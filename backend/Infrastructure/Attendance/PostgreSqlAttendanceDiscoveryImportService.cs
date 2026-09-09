using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using BDIP.Persistence.PostgreSQL;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System.Net.Http.Json;
using System.Text.Json;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlAttendanceDiscoveryImportService
    : IAttendanceDiscoveryImportService
{
    private readonly ApplicationDbOptions _options;
    private readonly HttpClient _httpClient;

    public PostgreSqlAttendanceDiscoveryImportService(
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
            ApplicationName = "BDIP Attendance Discovery Import"
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task<AttendanceDiscoveryImportResponse> ImportAsync(
        AttendanceDiscoveryImportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.MachineCode))
        {
            throw new ArgumentException(
                "MachineCode is required.",
                nameof(request));
        }

        var selectedFingerIds = request.FingerIds
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (selectedFingerIds.Count == 0)
        {
            throw new ArgumentException(
                "Pilih minimal satu FingerID untuk di-import.",
                nameof(request));
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
                request.MachineCode.Trim());

            await using var reader =
                await machineCommand.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new KeyNotFoundException(
                    $"Finger machine '{request.MachineCode}' not found.");
            }

            machineId = reader.GetGuid(0);
            code = reader.GetString(1);
            name = reader["name"]?.ToString() ?? string.Empty;
            ipAddress = reader["ip_address"]?.ToString() ?? string.Empty;
            port = Convert.ToInt32(reader["port"]);
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            throw new InvalidOperationException(
                $"Finger machine '{code}' does not have an IP address.");
        }

        using var response = await _httpClient.PostAsJsonAsync(
            "http://192.168.100.129:8090/attendance/preview",
            new
            {
                ip = ipAddress,
                port
            },
            cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(cancellationToken);

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

        var deviceUsers = deviceResult.Users ?? new List<DevicePreviewUser>();
        var deviceTemplates =
            deviceResult.Templates ?? new List<DevicePreviewTemplate>();

        var selectedUsers = deviceUsers
            .Where(user =>
                !string.IsNullOrWhiteSpace(user.UserId) &&
                selectedFingerIds.Contains(
                    user.UserId.Trim(),
                    StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (selectedUsers.Count != selectedFingerIds.Count)
        {
            throw new ArgumentException(
                "Satu atau lebih FingerID yang dipilih tidak ditemukan pada mesin.");
        }

        var templateGroups = deviceTemplates
            .GroupBy(template => template.Uid)
            .ToDictionary(
                group => group.Key,
                group => group.ToList());

        await using var connection =
            await dataSource.OpenConnectionAsync(cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        var importedUserCount = 0;
        var importedTemplateCount = 0;
        var skippedAlreadyMatchedCount = 0;

        foreach (var deviceUser in selectedUsers)
        {
            var fingerId = deviceUser.UserId!.Trim();

            Guid? existingUserId = null;

            await using (var existingCommand =
                new NpgsqlCommand(
                    """
                    SELECT id
                    FROM public.users
                    WHERE LOWER(TRIM(finger_id)) = LOWER(@finger_id)
                    LIMIT 1;
                    """,
                    connection,
                    transaction))
            {
                existingCommand.Parameters.AddWithValue(
                    "finger_id",
                    fingerId);

                var existing = await existingCommand.ExecuteScalarAsync(
                    cancellationToken);

                if (existing is Guid guid)
                {
                    existingUserId = guid;
                }
            }

            Guid newUserId;

            if (existingUserId.HasValue)
            {
                skippedAlreadyMatchedCount++;
                newUserId = existingUserId.Value;
            }
            else
            {
                var fullName = string.IsNullOrWhiteSpace(deviceUser.Name)
                    ? fingerId
                    : deviceUser.Name.Trim();

                newUserId = Guid.NewGuid();

                await using (var userCommand =
                    new NpgsqlCommand(
                    """
                    INSERT INTO public.users
                    (
                        id,
                        username,
                        full_name,
                        email,
                        unit_id,
                        enabled,
                        created_at,
                        updated_at,
                        finger_id
                    )
                    VALUES
                    (
                        @id,
                        @username,
                        @full_name,
                        '',
                        NULL,
                        TRUE,
                        NOW(),
                        NOW(),
                        @finger_id
                    );
                    """,
                    connection,
                    transaction))
            {
                userCommand.Parameters.Add(
                    "id",
                    NpgsqlDbType.Uuid).Value = newUserId;

                userCommand.Parameters.Add(
                    "username",
                    NpgsqlDbType.Varchar).Value = fingerId;

                userCommand.Parameters.Add(
                    "full_name",
                    NpgsqlDbType.Varchar).Value = fullName;

                userCommand.Parameters.Add(
                    "finger_id",
                    NpgsqlDbType.Varchar).Value = fingerId;

                await userCommand.ExecuteNonQueryAsync(
                    cancellationToken);
                }
            }

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
                    NpgsqlDbType.Uuid).Value = newUserId;

                machineUserCommand.Parameters.Add(
                    "finger_id",
                    NpgsqlDbType.Varchar).Value = fingerId;

                machineUserCommand.Parameters.Add(
                    "device_uid",
                    NpgsqlDbType.Integer).Value = deviceUser.Uid;

                machineUserCommand.Parameters.Add(
                    "device_user_id",
                    NpgsqlDbType.Varchar).Value = fingerId;

                machineUserCommand.Parameters.Add(
                    "device_name",
                    NpgsqlDbType.Varchar).Value =
                    deviceUser.Name ?? string.Empty;

                machineUserCommand.Parameters.Add(
                    "privilege",
                    NpgsqlDbType.Smallint).Value =
                    (short)deviceUser.Privilege;

                await machineUserCommand.ExecuteNonQueryAsync(
                    cancellationToken);
            }

            importedUserCount++;

            if (!templateGroups.TryGetValue(
                    deviceUser.Uid,
                    out var templatesForUser))
            {
                continue;
            }

            foreach (var template in templatesForUser)
            {
                if (template.Fid < 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid fingerprint FID '{template.Fid}' for device UID '{deviceUser.Uid}'.");
                }

                if (string.IsNullOrWhiteSpace(template.Template))
                {
                    continue;
                }

                byte[] templateData;

                try
                {
                    templateData =
                        Convert.FromHexString(template.Template.Trim());
                }
                catch (FormatException ex)
                {
                    throw new InvalidOperationException(
                        $"Invalid fingerprint template data for device UID '{deviceUser.Uid}', FID '{template.Fid}'.",
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
                        DO NOTHING;
                        """,
                        connection,
                        transaction);

                templateCommand.Parameters.Add(
                    "user_id",
                    NpgsqlDbType.Uuid).Value = newUserId;

                templateCommand.Parameters.Add(
                    "finger_id",
                    NpgsqlDbType.Varchar).Value = fingerId;

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

        return new AttendanceDiscoveryImportResponse
        {
            Success = true,
            MachineCode = code,
            MachineName = name,
            ImportedUserCount = importedUserCount,
            ImportedTemplateCount = importedTemplateCount,
            SkippedAlreadyMatchedCount = skippedAlreadyMatchedCount
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
