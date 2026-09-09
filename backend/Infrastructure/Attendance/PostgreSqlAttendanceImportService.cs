using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;
using NpgsqlTypes;

using System.Net.Http.Json;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlAttendanceImportService
    : IAttendanceImportService
{
    private readonly ApplicationDbOptions _options;
    private readonly HttpClient _httpClient;

    public PostgreSqlAttendanceImportService(
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
            ApplicationName = "BDIP Attendance Import"
        };

        return NpgsqlDataSource.Create(
            builder.ConnectionString);
    }

    public async Task<AttendanceImportResponse> ImportAsync(
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
        Guid machineId;

        await using (var machineCommand = dataSource.CreateCommand())
        {
            machineCommand.CommandText = """
                SELECT
                    id,
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

            machineId = machineReader.GetGuid(0);
            code = machineReader.GetString(1);
            name = machineReader["name"]?.ToString() ?? string.Empty;
            ipAddress =
                machineReader["ip_address"]?.ToString()
                ?? string.Empty;
            port = Convert.ToInt32(machineReader["port"]);
        }

        bool isMasterMachine;

        await using (var masterCommand = dataSource.CreateCommand())
        {
            masterCommand.CommandText = """
                SELECT EXISTS
                (
                    SELECT 1
                    FROM public.attendance_settings
                    WHERE master_machine_id = @machine_id
                );
                """;

            masterCommand.Parameters.Add(
                "machine_id",
                NpgsqlDbType.Uuid).Value = machineId;

            isMasterMachine =
                (bool)(await masterCommand.ExecuteScalarAsync(
                    cancellationToken))!;
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
                ?? "Attendance collector returned an empty or unsuccessful response.");
        }

        var deviceUsers =
            deviceResult.Users ?? new List<DevicePreviewUser>();

        var deviceTemplates =
            deviceResult.Templates ?? new List<DevicePreviewTemplate>();

        var deviceUserIds =
            deviceUsers
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
                    .Select(value => value!.ToLowerInvariant())
                    .ToArray());

            await using var userReader =
                await userCommand.ExecuteReaderAsync(
                    cancellationToken);

            while (await userReader.ReadAsync(cancellationToken))
            {
                var userId = userReader.GetGuid(0);
                var fingerId = userReader.GetString(1).Trim();

                bdipUsers[fingerId] = userId;
            }
        }

        var matchedUsers =
            deviceUsers
                .Select(user =>
                {
                    var deviceUserId =
                        user.UserId?.Trim() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(deviceUserId))
                    {
                        return null;
                    }

                    if (!bdipUsers.TryGetValue(
                            deviceUserId,
                            out var userId))
                    {
                        return null;
                    }

                    return new MatchedDeviceUser(
                        user,
                        userId,
                        deviceUserId,
                        deviceUserId);
                })
                .Where(value => value is not null)
                .Select(value => value!)
                .ToList();

        var templateGroups =
            deviceTemplates
                .GroupBy(template => template.Uid)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList());

        var skippedUnmatched =
            deviceUsers
                .Select(user => user.UserId?.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(value => !bdipUsers.ContainsKey(value!));

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(
                cancellationToken);

        var importedUserCount = 0;
        var importedTemplateCount = 0;

        foreach (var matched in matchedUsers)
        {
            await using (var machineUserCommand =
                new NpgsqlCommand(
                    """
                    INSERT INTO public.attendance_finger_machine_users
                    (
                        finger_machine_id,
                        user_id,
                        finger_id,
                        device_uid,
                        device_user_id,
                        device_name,
                        privilege,
                        enabled,
                        last_seen_at,
                        created_at,
                        updated_at
                    )
                    VALUES
                    (
                        @finger_machine_id,
                        @user_id,
                        @finger_id,
                        @device_uid,
                        @device_user_id,
                        @device_name,
                        @privilege,
                        TRUE,
                        NOW(),
                        NOW(),
                        NOW()
                    )
                    ON CONFLICT (finger_machine_id, user_id)
                    DO UPDATE SET
                        finger_id = EXCLUDED.finger_id,
                        device_uid = EXCLUDED.device_uid,
                        device_user_id = EXCLUDED.device_user_id,
                        device_name = EXCLUDED.device_name,
                        privilege = EXCLUDED.privilege,
                        last_seen_at = NOW(),
                        updated_at = NOW();
                    """,
                    connection,
                    transaction))
            {
                machineUserCommand.Parameters.Add(
                    "finger_machine_id",
                    NpgsqlDbType.Uuid).Value = machineId;

                machineUserCommand.Parameters.Add(
                    "user_id",
                    NpgsqlDbType.Uuid).Value = matched.UserId;

                machineUserCommand.Parameters.Add(
                    "finger_id",
                    NpgsqlDbType.Varchar).Value = matched.FingerId;

                machineUserCommand.Parameters.Add(
                    "device_uid",
                    NpgsqlDbType.Integer).Value = matched.User.Uid;

                machineUserCommand.Parameters.Add(
                    "device_user_id",
                    NpgsqlDbType.Varchar).Value = matched.DeviceUserId;

                machineUserCommand.Parameters.Add(
                    "device_name",
                    NpgsqlDbType.Varchar).Value =
                    matched.User.Name ?? string.Empty;

                machineUserCommand.Parameters.Add(
                    "privilege",
                    NpgsqlDbType.Smallint).Value =
                    (short)matched.User.Privilege;

                await machineUserCommand.ExecuteNonQueryAsync(
                    cancellationToken);
            }

            importedUserCount++;

            // Hanya Master Machine yang boleh menjadi sumber
            // fingerprint template Master Attendance.
            //
            // Mesin non-Master hanya melakukan reconciliation
            // user-machine dan TIDAK boleh menimpa template Master.
            if (!isMasterMachine)
            {
                continue;
            }

            if (!templateGroups.TryGetValue(
                    matched.User.Uid,
                    out var templatesForUser))
            {
                continue;
            }

            foreach (var template in templatesForUser)
            {
                if (template.Fid < 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid fingerprint FID '{template.Fid}' for device UID '{matched.User.Uid}'.");
                }

                if (string.IsNullOrWhiteSpace(template.Template))
                {
                    continue;
                }

                byte[] templateData;

                try
                {
                    templateData =
                        Convert.FromHexString(
                            template.Template.Trim());
                }
                catch (FormatException ex)
                {
                    throw new InvalidOperationException(
                        $"Invalid fingerprint template data for device UID '{matched.User.Uid}', FID '{template.Fid}'.",
                        ex);
                }

                await using var templateCommand =
                    new NpgsqlCommand(
                        """
                        INSERT INTO public.attendance_finger_templates
                        (
                            user_id,
                            finger_id,
                            fid,
                            valid,
                            template_data,
                            created_at,
                            updated_at
                        )
                        VALUES
                        (
                            @user_id,
                            @finger_id,
                            @fid,
                            @valid,
                            @template_data,
                            NOW(),
                            NOW()
                        )
                        ON CONFLICT (user_id, fid)
                        DO UPDATE SET
                            finger_id = EXCLUDED.finger_id,
                            valid = EXCLUDED.valid,
                            template_data = EXCLUDED.template_data,
                            updated_at = NOW();
                        """,
                        connection,
                        transaction);

                templateCommand.Parameters.Add(
                    "user_id",
                    NpgsqlDbType.Uuid).Value = matched.UserId;

                templateCommand.Parameters.Add(
                    "finger_id",
                    NpgsqlDbType.Varchar).Value = matched.FingerId;

                templateCommand.Parameters.Add(
                    "fid",
                    NpgsqlDbType.Smallint).Value =
                    (short)template.Fid;

                templateCommand.Parameters.Add(
                    "valid",
                    NpgsqlDbType.Boolean).Value =
                    template.Valid != 0;

                templateCommand.Parameters.Add(
                    "template_data",
                    NpgsqlDbType.Bytea).Value =
                    templateData;

                await templateCommand.ExecuteNonQueryAsync(
                    cancellationToken);

                importedTemplateCount++;
            }
        }

        await transaction.CommitAsync(cancellationToken);

        return new AttendanceImportResponse
        {
            Success = true,
            MachineCode = code,
            MachineName = name,
            ImportedUserCount = importedUserCount,
            ImportedTemplateCount = importedTemplateCount,
            SkippedUnmatchedUserCount = skippedUnmatched
        };
    }

    private sealed record MatchedDeviceUser(
        DevicePreviewUser User,
        Guid UserId,
        string FingerId,
        string DeviceUserId);

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
