using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

using System.Net.Http.Json;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlAttendancePreviewService
    : IAttendancePreviewService
{
    private readonly ApplicationDbOptions _options;
    private readonly HttpClient _httpClient;

    public PostgreSqlAttendancePreviewService(
        IOptions<ApplicationDbOptions> options,
        HttpClient httpClient)
    {
        _options = options.Value;
        _httpClient = httpClient;
    }

    private NpgsqlDataSource CreateDataSource()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _options.Host,
            Port = _options.Port,
            Database = _options.Database,
            Username = _options.Username,
            Password = _options.Password,
            SslMode = SslMode.Disable,
            Timeout = 10,
            CommandTimeout = 15,
            ApplicationName = "BDIP Attendance Preview"
        };

        return NpgsqlDataSource.Create(
            builder.ConnectionString);
    }

    public async Task<AttendancePreviewResponse> PreviewAsync(
        string machineCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
        {
            throw new ArgumentException(
                "Machine code is required.",
                nameof(machineCode));
        }

        await using var dataSource = CreateDataSource();

        await using var machineCommand =
            dataSource.CreateCommand();

        machineCommand.CommandText = """
            SELECT
                code,
                name,
                ip_address,
                port
            FROM public.finger_machines
            WHERE LOWER(code) = LOWER(@code)
            LIMIT 1;
            """;

        machineCommand.Parameters.AddWithValue(
            "code",
            machineCode.Trim());

        await using var machineReader =
            await machineCommand.ExecuteReaderAsync(
                cancellationToken);

        if (!await machineReader.ReadAsync(cancellationToken))
        {
            throw new KeyNotFoundException(
                $"Finger machine '{machineCode}' not found.");
        }

        var code =
            machineReader["code"]?.ToString()
            ?? machineCode.Trim();

        var name =
            machineReader["name"]?.ToString()
            ?? string.Empty;

        var ipAddress =
            machineReader["ip_address"]?.ToString()
            ?? string.Empty;

        var port =
            Convert.ToInt32(machineReader["port"]);

        await machineReader.CloseAsync();

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            throw new InvalidOperationException(
                $"Finger machine '{code}' does not have an IP address.");
        }

        var payload = new
        {
            ip = ipAddress,
            port = port
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "http://192.168.100.129:8090/attendance/preview",
                payload,
                cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var deviceResult =
            System.Text.Json.JsonSerializer.Deserialize<DevicePreviewResult>(
                responseBody,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


        if (deviceResult is null || !deviceResult.Success)
        {
            throw new InvalidOperationException(
                deviceResult?.Error
                ?? "Attendance preview returned an empty or unsuccessful response.");
        }

        var users =
            deviceResult.Users ?? new List<DevicePreviewUser>();

        var templates =
            deviceResult.Templates ?? new List<DevicePreviewTemplate>();

        var templateCounts =
            templates
                .GroupBy(template => template.Uid)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count());

        var deviceUserIds =
            users
                .Select(user => user.UserId?.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        var bdipUsers =
            new Dictionary<string, Guid>(
                StringComparer.OrdinalIgnoreCase);

        if (deviceUserIds.Count > 0)
        {
            await using var userCommand =
                dataSource.CreateCommand();

            userCommand.CommandText = """
                SELECT
                    id,
                    finger_id
                FROM public.users
                WHERE NULLIF(TRIM(finger_id), '') IS NOT NULL
                  AND LOWER(TRIM(finger_id)) = ANY(@finger_ids);
                """;

            userCommand.Parameters.AddWithValue(
                "finger_ids",
                deviceUserIds
                    .Select(value => value.ToLowerInvariant())
                    .ToArray());

            await using var userReader =
                await userCommand.ExecuteReaderAsync(
                    cancellationToken);

            while (await userReader.ReadAsync(cancellationToken))
            {
                var userId =
                    userReader.GetGuid(0);

                var fingerId =
                    userReader.GetString(1).Trim();

                bdipUsers[fingerId] = userId;
            }
        }

        var previewUsers =
            new List<AttendancePreviewUser>();

        foreach (var user in users)
        {
            var deviceUserId =
                user.UserId?.Trim() ?? string.Empty;

            var matched =
                !string.IsNullOrWhiteSpace(deviceUserId)
                && bdipUsers.ContainsKey(deviceUserId);

            var templateCount =
                templateCounts.TryGetValue(
                    user.Uid,
                    out var count)
                    ? count
                    : 0;

            previewUsers.Add(
                new AttendancePreviewUser
                {
                    DeviceUid = user.Uid,
                    DeviceUserId = deviceUserId,
                    DeviceName = user.Name ?? string.Empty,
                    FingerId = matched
                        ? deviceUserId
                        : string.Empty,
                    Matched = matched,
                    TemplateCount = templateCount
                });
        }

        return new AttendancePreviewResponse
        {
            Success = true,
            MachineCode = code,
            MachineName = name,
            DeviceUserCount = users.Count,
            DeviceTemplateCount = templates.Count,
            MatchedUserCount =
                previewUsers.Count(user => user.Matched),
            UnmatchedUserCount =
                previewUsers.Count(user => !user.Matched),
            Users = previewUsers
        };
    }

    private sealed class DevicePreviewResult
    {
        public bool Success { get; set; }

        public string? Error { get; set; }

        public List<DevicePreviewUser>? Users { get; set; }

        public List<DevicePreviewTemplate>? Templates { get; set; }
    }

    private sealed class DevicePreviewUser
    {
        public int Uid { get; set; }

        public string? Name { get; set; }

        public string? UserId { get; set; }

        public int Privilege { get; set; }

        public string? GroupId { get; set; }

        public int Card { get; set; }
    }

    private sealed class DevicePreviewTemplate
    {
        public int Uid { get; set; }

        public int Fid { get; set; }

        public int Valid { get; set; }

        public int Size { get; set; }

        public string? Template { get; set; }
    }
}
