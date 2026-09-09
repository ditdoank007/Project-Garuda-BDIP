using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

using System.Net.Http.Json;
using System.Text.Json;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlAttendanceDiscoveryService
    : IAttendanceDiscoveryService
{
    private readonly ApplicationDbOptions _options;
    private readonly HttpClient _httpClient;

    public PostgreSqlAttendanceDiscoveryService(
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
            CommandTimeout = 30,
            ApplicationName = "BDIP Attendance Discovery"
        };

        return NpgsqlDataSource.Create(
            builder.ConnectionString);
    }

    public async Task<AttendanceDiscoveryResponse> DiscoverAsync(
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

        string code;
        string name;
        string ipAddress;
        int port;

        await using (var machineCommand = dataSource.CreateCommand())
        {
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

            code = machineReader.GetString(0);

            name =
                machineReader["name"]?.ToString()
                ?? string.Empty;

            ipAddress =
                machineReader["ip_address"]?.ToString()
                ?? string.Empty;

            port =
                Convert.ToInt32(
                    machineReader["port"]);
        }

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
            JsonSerializer.Deserialize<DevicePreviewResult>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (deviceResult is null || !deviceResult.Success)
        {
            throw new InvalidOperationException(
                deviceResult?.Error
                ?? "Attendance collector returned an empty or unsuccessful response.");
        }

        var deviceUsers =
            deviceResult.Users
            ?? new List<DevicePreviewUser>();

        var deviceTemplates =
            deviceResult.Templates
            ?? new List<DevicePreviewTemplate>();

        var deviceUserIds =
            deviceUsers
                .Select(user => user.UserId?.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        var bdipUsers =
            new Dictionary<string, BdipUser>(
                StringComparer.OrdinalIgnoreCase);

        if (deviceUserIds.Count > 0)
        {
            await using var userCommand =
                dataSource.CreateCommand();

            userCommand.CommandText = """
                SELECT
                    id,
                    finger_id,
                    full_name
                FROM public.users
                WHERE NULLIF(TRIM(finger_id), '') IS NOT NULL
                  AND LOWER(TRIM(finger_id)) = ANY(@finger_ids);
                """;

            userCommand.Parameters.AddWithValue(
                "finger_ids",
                deviceUserIds
                    .Select(value => value!.ToLowerInvariant())
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

                var fullName =
                    userReader["full_name"]?.ToString()
                    ?? string.Empty;

                bdipUsers[fingerId] =
                    new BdipUser(
                        userId,
                        fingerId,
                        fullName);
            }
        }

        var templateGroups =
            deviceTemplates
                .GroupBy(template => template.Uid)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count());

        var discoveryUsers =
            deviceUsers
                .Where(user =>
                    !string.IsNullOrWhiteSpace(
                        user.UserId))
                .Select(user =>
                {
                    var fingerId =
                        user.UserId!.Trim();

                    var matched =
                        bdipUsers.TryGetValue(
                            fingerId,
                            out var bdipUser);

                    var templateCount =
                        templateGroups.TryGetValue(
                            user.Uid,
                            out var count)
                            ? count
                            : 0;

                    return new AttendanceDiscoveryUser
                    {
                        DeviceUid = user.Uid,

                        FingerId = fingerId,

                        DeviceName =
                            user.Name?.Trim()
                            ?? string.Empty,

                        BdipFullName =
                            matched
                                ? bdipUser!.FullName
                                : string.Empty,

                        IsMatched = matched,

                        TemplateCount =
                            templateCount
                    };
                })
                .OrderBy(user => user.FingerId)
                .ToList();

        return new AttendanceDiscoveryResponse
        {
            Success = true,

            MachineCode = code,

            MachineName = name,

            DeviceUserCount =
                deviceUsers.Count,

            DeviceTemplateCount =
                deviceTemplates.Count,

            MatchedUserCount =
                discoveryUsers.Count(user => user.IsMatched),

            NewUserCount =
                discoveryUsers.Count(user => !user.IsMatched),

            Users = discoveryUsers
        };
    }

    public async Task<AttendanceDiscoveryDeleteResponse> DeleteAsync(
        AttendanceDiscoveryDeleteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentException(
                "Delete request is required.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.MachineCode))
        {
            throw new ArgumentException(
                "Machine code is required.",
                nameof(request));
        }

        var fingerIds =
            request.FingerIds
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        if (fingerIds.Count == 0)
        {
            throw new ArgumentException(
                "At least one FingerID is required.",
                nameof(request));
        }

        await using var dataSource = CreateDataSource();

        string code;
        string name;
        string ipAddress;
        int port;

        await using (var machineCommand = dataSource.CreateCommand())
        {
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
                request.MachineCode.Trim());

            await using var machineReader =
                await machineCommand.ExecuteReaderAsync(
                    cancellationToken);

            if (!await machineReader.ReadAsync(cancellationToken))
            {
                throw new KeyNotFoundException(
                    $"Finger machine '{request.MachineCode}' not found.");
            }

            code = machineReader.GetString(0);

            name =
                machineReader["name"]?.ToString()
                ?? string.Empty;

            ipAddress =
                machineReader["ip_address"]?.ToString()
                ?? string.Empty;

            port =
                Convert.ToInt32(
                    machineReader["port"]);
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            throw new InvalidOperationException(
                $"Finger machine '{code}' does not have an IP address.");
        }

        // Baca kondisi mesin terbaru agar DeviceUid tidak bergantung
        // pada data yang mungkin sudah stale di browser.
        var previewPayload = new
        {
            ip = ipAddress,
            port = port
        };

        using var previewResponse =
            await _httpClient.PostAsJsonAsync(
                "http://192.168.100.129:8090/attendance/preview",
                previewPayload,
                cancellationToken);

        var previewBody =
            await previewResponse.Content.ReadAsStringAsync(
                cancellationToken);

        previewResponse.EnsureSuccessStatusCode();

        var deviceResult =
            JsonSerializer.Deserialize<DevicePreviewResult>(
                previewBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (deviceResult is null || !deviceResult.Success)
        {
            throw new InvalidOperationException(
                deviceResult?.Error
                ?? "Attendance collector returned an empty or unsuccessful response.");
        }

        var deviceUsers =
            deviceResult.Users
            ?? new List<DevicePreviewUser>();

        var deviceUsersByFingerId =
            deviceUsers
                .Where(user =>
                    !string.IsNullOrWhiteSpace(user.UserId))
                .GroupBy(
                    user => user.UserId!.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.OrdinalIgnoreCase);

        var results =
            new List<AttendanceDiscoveryDeleteResult>();

        foreach (var fingerId in fingerIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!deviceUsersByFingerId.TryGetValue(
                    fingerId,
                    out var deviceUser))
            {
                results.Add(new AttendanceDiscoveryDeleteResult
                {
                    FingerId = fingerId,
                    DeviceUid = 0,
                    DeviceName = string.Empty,
                    Success = true,
                    Deleted = false,
                    Verified = true,
                    Error = "FingerID tidak ditemukan pada mesin. Tidak ada yang dihapus."
                });

                continue;
            }

            var deletePayload = new
            {
                ip = ipAddress,
                port = port,
                uid = deviceUser.Uid
            };

            try
            {
                using var deleteResponse =
                    await _httpClient.PostAsJsonAsync(
                        "http://192.168.100.129:8090/attendance/delete-user",
                        deletePayload,
                        cancellationToken);

                var deleteBody =
                    await deleteResponse.Content.ReadAsStringAsync(
                        cancellationToken);

                AttendanceDeviceDeleteResult? deleteResult = null;

                try
                {
                    deleteResult =
                        JsonSerializer.Deserialize<AttendanceDeviceDeleteResult>(
                            deleteBody,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                }
                catch (JsonException)
                {
                    // Body bukan JSON; error HTTP akan ditangani di bawah.
                }

                if (!deleteResponse.IsSuccessStatusCode)
                {
                    results.Add(new AttendanceDiscoveryDeleteResult
                    {
                        FingerId = fingerId,
                        DeviceUid = deviceUser.Uid,
                        DeviceName =
                            deviceUser.Name?.Trim()
                            ?? string.Empty,
                        Success = false,
                        Deleted =
                            deleteResult?.Deleted
                            ?? false,
                        Verified =
                            deleteResult?.Verified
                            ?? false,
                        Error =
                            deleteResult?.Error
                            ?? $"Collector returned HTTP {(int)deleteResponse.StatusCode}."
                    });

                    continue;
                }

                results.Add(new AttendanceDiscoveryDeleteResult
                {
                    FingerId = fingerId,
                    DeviceUid = deviceUser.Uid,
                    DeviceName =
                        deviceUser.Name?.Trim()
                        ?? string.Empty,
                    Success =
                        deleteResult?.Success
                        ?? false,
                    Deleted =
                        deleteResult?.Deleted
                        ?? false,
                    Verified =
                        deleteResult?.Verified
                        ?? false,
                    Error =
                        deleteResult?.Error
                        ?? string.Empty
                });
            }
            catch (Exception ex)
                when (ex is HttpRequestException
                    or TaskCanceledException)
            {
                results.Add(new AttendanceDiscoveryDeleteResult
                {
                    FingerId = fingerId,
                    DeviceUid = deviceUser.Uid,
                    DeviceName =
                        deviceUser.Name?.Trim()
                        ?? string.Empty,
                    Success = false,
                    Deleted = false,
                    Verified = false,
                    Error = ex.Message
                });
            }
        }

        var deletedCount =
            results.Count(result =>
                result.Success
                && result.Deleted
                && result.Verified);

        var failedCount =
            results.Count(result =>
                !result.Success
                || (result.Deleted && !result.Verified));

        var skippedCount =
            results.Count(result =>
                result.Success
                && !result.Deleted);

        return new AttendanceDiscoveryDeleteResponse
        {
            Success = failedCount == 0,

            MachineCode = code,

            MachineName = name,

            RequestedCount = fingerIds.Count,

            DeletedCount = deletedCount,

            FailedCount = failedCount,

            SkippedCount = skippedCount,

            Results = results
        };
    }

    private sealed record BdipUser(
        Guid UserId,
        string FingerId,
        string FullName);

    private sealed class AttendanceDeviceDeleteResult
    {
        public bool Success { get; set; }

        public int Uid { get; set; }

        public string? UserId { get; set; }

        public string? Name { get; set; }

        public bool Deleted { get; set; }

        public bool Verified { get; set; }

        public string? Error { get; set; }
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
