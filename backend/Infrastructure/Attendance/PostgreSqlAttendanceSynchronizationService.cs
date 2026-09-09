using System.Net.Http.Json;
using System.Text.Json;

using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlAttendanceSynchronizationService
    : IAttendanceSynchronizationService
{
    private readonly ApplicationDbOptions _options;
    private readonly HttpClient _httpClient;

    public PostgreSqlAttendanceSynchronizationService(
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
            CommandTimeout = 60,
            ApplicationName = "BDIP Attendance Synchronization"
        };

        return NpgsqlDataSource.Create(
            builder.ConnectionString);
    }

    public async Task<AttendanceSynchronizationResponse> SyncNowAsync(
        string? fingerId = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.Now;

        var response =
            new AttendanceSynchronizationResponse
            {
                StartedAt = startedAt
            };

        await using var dataSource =
            CreateDataSource();

        var machines =
            await LoadActiveMachinesAsync(
                dataSource,
                cancellationToken);

        var masterUsers =
            await LoadMasterUsersAsync(
                dataSource,
                cancellationToken);

        var masterTemplates =
            await LoadMasterTemplatesAsync(
                dataSource,
                cancellationToken);

        var requestedFingerId =
            fingerId?.Trim();

        if (!string.IsNullOrWhiteSpace(requestedFingerId))
        {
            masterUsers =
                masterUsers
                    .Where(x =>
                        string.Equals(
                            x.FingerId,
                            requestedFingerId,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (masterUsers.Count == 0)
            {
                throw new KeyNotFoundException(
                    $"FingerID '{requestedFingerId}' tidak ditemukan di Master Attendance.");
            }
        }

        response.MachineCount = machines.Count;

        foreach (var machine in machines)
        {
            var result =
                await SyncMachineAsync(
                    dataSource,
                    machine,
                    masterUsers,
                    masterTemplates,
                    cancellationToken);

            response.Machines.Add(result);
        }

        response.SuccessMachineCount =
            response.Machines.Count(
                x => x.Status == "SUCCESS");

        response.PendingMachineCount =
            response.Machines.Count(
                x => x.Status == "PENDING");

        response.FailedMachineCount =
            response.Machines.Count(
                x => x.Status == "FAILED");

        response.FinishedAt =
            DateTimeOffset.Now;

        response.Success =
            response.FailedMachineCount == 0;

        return response;
    }

    private async Task<List<FingerMachineDto>>
        LoadActiveMachinesAsync(
            NpgsqlDataSource dataSource,
            CancellationToken cancellationToken)
    {
        var machines =
            new List<FingerMachineDto>();

        await using var command =
            dataSource.CreateCommand();

        command.CommandText = """
            SELECT
                id,
                code,
                name,
                ip_address,
                port
            FROM public.finger_machines
            WHERE is_active = TRUE
            ORDER BY code;
            """;

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        while (await reader.ReadAsync(
            cancellationToken))
        {
            machines.Add(
                new FingerMachineDto
                {
                    Id = reader.GetGuid(0),
                    Code = reader.GetString(1),
                    Name = reader.GetString(2),
                    IpAddress =
                        reader.IsDBNull(3)
                            ? null
                            : reader.GetString(3),
                    Port =
                        reader.IsDBNull(4)
                            ? 4370
                            : reader.GetInt32(4)
                });
        }

        return machines;
    }

    private async Task<List<MasterUserDto>>
        LoadMasterUsersAsync(
            NpgsqlDataSource dataSource,
            CancellationToken cancellationToken)
    {
        var users =
            new List<MasterUserDto>();

        await using var command =
            dataSource.CreateCommand();

        command.CommandText = """
            SELECT
                id,
                finger_id,
                full_name,
                enabled
            FROM public.users
            WHERE finger_id IS NOT NULL
              AND TRIM(finger_id) <> ''
            ORDER BY finger_id;
            """;

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        while (await reader.ReadAsync(
            cancellationToken))
        {
            users.Add(
                new MasterUserDto
                {
                    Id = reader.GetGuid(0),
                    FingerId =
                        reader.GetString(1).Trim(),
                    FullName =
                        reader.IsDBNull(2)
                            ? string.Empty
                            : reader.GetString(2),
                    Enabled =
                        reader.GetBoolean(3)
                });
        }

        return users;
    }

    private async Task<
        Dictionary<string, List<MasterTemplateDto>>>
        LoadMasterTemplatesAsync(
            NpgsqlDataSource dataSource,
            CancellationToken cancellationToken)
    {
        var templates =
            new Dictionary<
                string,
                List<MasterTemplateDto>>(
                StringComparer.OrdinalIgnoreCase);

        await using var command =
            dataSource.CreateCommand();

        command.CommandText = """
            SELECT
                u.finger_id,
                t.fid,
                encode(t.template_data, 'hex')
            FROM public.attendance_finger_templates t
            INNER JOIN public.users u
                ON u.id = t.user_id
            WHERE u.finger_id IS NOT NULL
              AND TRIM(u.finger_id) <> ''
              AND t.valid = TRUE
            ORDER BY
                u.finger_id,
                t.fid;
            """;

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        while (await reader.ReadAsync(
            cancellationToken))
        {
            var fingerId =
                reader.GetString(0).Trim();

            var template =
                new MasterTemplateDto
                {
                    Fid = reader.GetInt16(1),
                    Template =
                        reader.GetString(2)
                };

            if (!templates.TryGetValue(
                fingerId,
                out var list))
            {
                list = [];
                templates[fingerId] = list;
            }

            list.Add(template);
        }

        return templates;
    }

    private async Task<
        AttendanceSynchronizationMachineResult>
        SyncMachineAsync(
            NpgsqlDataSource dataSource,
            FingerMachineDto machine,
            List<MasterUserDto> masterUsers,
            Dictionary<string, List<MasterTemplateDto>>
                masterTemplates,
            CancellationToken cancellationToken)
    {
        var result =
            new AttendanceSynchronizationMachineResult
            {
                MachineCode = machine.Code,
                MachineName = machine.Name,
                Status = "PENDING"
            };

        if (string.IsNullOrWhiteSpace(
            machine.IpAddress))
        {
            result.Status = "FAILED";
            result.Message =
                "IP mesin kosong.";

            return result;
        }

        try
        {
            var snapshot =
                await ReadMachineSnapshotAsync(
                    machine,
                    cancellationToken);

            if (!snapshot.Success)
            {
                result.Status = "PENDING";
                result.Message =
                    "Mesin tidak dapat dibaca oleh collector.";

                return result;
            }

            var machineUsers =
                snapshot.Users ??
                [];

            var machineByFingerId =
                machineUsers
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(
                            x.UserId))
                    .ToDictionary(
                        x => x.UserId!.Trim(),
                        x => x,
                        StringComparer.OrdinalIgnoreCase);

            var machineMatchedCount = 0;
            var createdCount = 0;
            var updatedCount = 0;
            var disabledCount = 0;
            var templateWrittenCount = 0;

            foreach (var masterUser in masterUsers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                masterTemplates.TryGetValue(
                    masterUser.FingerId,
                    out var templates);

                templates ??= [];

                var exists =
                    machineByFingerId.TryGetValue(
                        masterUser.FingerId,
                        out var machineUser);

                var templatesMatch =
                    exists &&
                    TemplatesMatch(
                        machineUser!,
                        templates);

                /*
                 * Jika user dan seluruh template sama,
                 * tidak perlu menulis ulang ke mesin.
                 */
                if (exists &&
                    templatesMatch &&
                    masterUser.Enabled)
                {
                    machineMatchedCount++;
                    continue;
                }

                /*
                 * Jika Master BDIP disabled, kita wajib
                 * meneruskan status disabled ke mesin.
                 *
                 * Jika Master enabled, kita tidak memaksa
                 * meng-enable user yang mungkin sengaja
                 * dinonaktifkan operator di mesin.
                 */
                var desiredEnabled =
                    masterUser.Enabled;

                var syncResponse =
                    await SyncUserToMachineAsync(
                        machine,
                        masterUser,
                        templates,
                        desiredEnabled,
                        cancellationToken);

                if (!syncResponse.Success)
                {
                    result.Status = "FAILED";
                    result.Message =
                        $"Gagal sinkronisasi FingerID " +
                        $"{masterUser.FingerId}: " +
                        syncResponse.Error;

                    return result;
                }

                if (syncResponse.Created)
                {
                    createdCount++;
                }

                if (syncResponse.Updated)
                {
                    updatedCount++;
                }

                templateWrittenCount +=
                    syncResponse.TemplatesWritten;

                if (!masterUser.Enabled)
                {
                    disabledCount++;
                }
            }

            await UpdateMachineRegistrationsAsync(
                dataSource,
                machine,
                masterUsers,
                machineUsers,
                cancellationToken);

            result.Status = "SUCCESS";
            result.Matched = machineMatchedCount;
            result.Created = createdCount;
            result.Updated = updatedCount;
            result.Disabled = disabledCount;
            result.TemplateWritten =
                templateWrittenCount;

            result.Deleted = 0;

            result.Message =
                "Master Attendance BDIP berhasil " +
                "diproses. Data EXTRA_ON_MACHINE " +
                "belum dihapus otomatis.";

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exc)
        {
            result.Status = "PENDING";
            result.Message =
                $"Mesin belum dapat diproses: " +
                $"{exc.Message}";

            return result;
        }
    }

    private async Task<MachineSnapshotDto>
        ReadMachineSnapshotAsync(
            FingerMachineDto machine,
            CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.PostAsJsonAsync(
                "http://192.168.100.129:8090/attendance/snapshot",
                new
                {
                    ip = machine.IpAddress,
                    port = machine.Port
                },
                cancellationToken);

        var body =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Collector HTTP {(int)response.StatusCode}: {body}");
        }

        var payload =
            JsonSerializer.Deserialize<MachineSnapshotDto>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (payload is null)
        {
            throw new InvalidOperationException(
                "Response snapshot collector kosong.");
        }

        return payload;
    }

    private async Task<SyncUserResponse>
        SyncUserToMachineAsync(
            FingerMachineDto machine,
            MasterUserDto masterUser,
            List<MasterTemplateDto> templates,
            bool enabled,
            CancellationToken cancellationToken)
    {
        var payload =
            new
            {
                ip = machine.IpAddress,
                port = machine.Port,
                userId = masterUser.FingerId,
                name = masterUser.FullName,
                enabled,
                templates = templates
                    .Select(x => new
                    {
                        fid = x.Fid,
                        valid = 1,
                        template = x.Template
                    })
                    .ToList()
            };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "http://192.168.100.129:8090/attendance/sync-user",
                payload,
                cancellationToken);

        var body =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        SyncUserResponse? result = null;

        try
        {
            result =
                JsonSerializer.Deserialize<SyncUserResponse>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
        }
        catch
        {
            // Ditangani di bawah sebagai response invalid.
        }

        if (!response.IsSuccessStatusCode)
        {
            return new SyncUserResponse
            {
                Success = false,
                Error =
                    $"HTTP {(int)response.StatusCode}: {body}"
            };
        }

        if (result is null)
        {
            return new SyncUserResponse
            {
                Success = false,
                Error =
                    "Response sync-user tidak valid."
            };
        }

        return result;
    }

    private static bool TemplatesMatch(
        MachineSnapshotUserDto machineUser,
        List<MasterTemplateDto> masterTemplates)
    {
        var machineTemplates =
            machineUser.Fingers ??
            [];

        if (machineTemplates.Count !=
            masterTemplates.Count)
        {
            return false;
        }

        foreach (var master in masterTemplates)
        {
            var machine =
                machineTemplates.FirstOrDefault(
                    x => x.Fid == master.Fid);

            if (machine is null)
            {
                return false;
            }

            if (!string.Equals(
                machine.Template ?? string.Empty,
                master.Template,
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static async Task
        UpdateMachineRegistrationsAsync(
            NpgsqlDataSource dataSource,
            FingerMachineDto machine,
            List<MasterUserDto> masterUsers,
            List<MachineSnapshotUserDto> machineUsers,
            CancellationToken cancellationToken)
    {
        /*
         * Registrasi BDIP diperbarui hanya untuk user
         * yang memang menjadi Master Attendance.
         *
         * Tidak ada DELETE terhadap data machine-only
         * pada tahap ini.
         */
        foreach (var user in masterUsers)
        {
            var machineUser =
                machineUsers
                    .FirstOrDefault(x =>
                        string.Equals(
                            x.UserId?.Trim(),
                            user.FingerId,
                            StringComparison.OrdinalIgnoreCase));

            if (machineUser is null)
            {
                continue;
            }

            await using var command =
                dataSource.CreateCommand();

            command.CommandText = """
                INSERT INTO
                    public.attendance_finger_machine_users
                (
                    finger_machine_id,
                    user_id,
                    finger_id,
                    device_uid,
                    device_user_id,
                    device_name,
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
                    @enabled,
                    NOW(),
                    NOW(),
                    NOW()
                )
                ON CONFLICT (finger_machine_id, user_id)
                DO UPDATE SET
                    finger_id = EXCLUDED.finger_id,
                    device_user_id =
                        COALESCE(
                            EXCLUDED.device_user_id,
                            attendance_finger_machine_users.device_user_id
                        ),
                    device_name =
                        EXCLUDED.device_name,
                    last_seen_at = NOW(),
                    updated_at = NOW();
                """;

            command.Parameters.AddWithValue(
                "finger_machine_id",
                machine.Id);

            command.Parameters.AddWithValue(
                "device_uid",
                machineUser.Uid);

            command.Parameters.AddWithValue(
                "user_id",
                user.Id);

            command.Parameters.AddWithValue(
                "finger_id",
                user.FingerId);

            command.Parameters.AddWithValue(
                "device_user_id",
                user.FingerId);

            command.Parameters.AddWithValue(
                "device_name",
                user.FullName);

            command.Parameters.AddWithValue(
                "enabled",
                user.Enabled);

            await command.ExecuteNonQueryAsync(
                cancellationToken);
        }
    }

    private sealed class FingerMachineDto
    {
        public Guid Id { get; init; }

        public string Code { get; init; } =
            string.Empty;

        public string Name { get; init; } =
            string.Empty;

        public string? IpAddress { get; init; }

        public int Port { get; init; } = 4370;
    }

    private sealed class MasterUserDto
    {
        public Guid Id { get; init; }

        public string FingerId { get; init; } =
            string.Empty;

        public string FullName { get; init; } =
            string.Empty;

        public bool Enabled { get; init; }
    }

    private sealed class MasterTemplateDto
    {
        public int Fid { get; init; }

        public string Template { get; init; } =
            string.Empty;
    }

    private sealed class MachineSnapshotDto
    {
        public bool Success { get; init; }

        public List<MachineSnapshotUserDto>? Users
            { get; init; }
    }

    private sealed class MachineSnapshotUserDto
    {
        public int Uid { get; init; }

        public string? UserId { get; init; }

        public string? Name { get; init; }

        public List<MachineSnapshotFingerDto>? Fingers
            { get; init; }
    }

    private sealed class MachineSnapshotFingerDto
    {
        public int Fid { get; init; }

        public string? Template { get; init; }
    }

    private sealed class SyncUserResponse
    {
        public bool Success { get; init; }

        public int Uid { get; init; }

        public string? UserId { get; init; }

        public string? Name { get; init; }

        public bool Enabled { get; init; }

        public bool Created { get; init; }

        public bool Updated { get; init; }

        public int TemplatesRequested { get; init; }

        public int TemplatesWritten { get; init; }

        public List<int> TemplateFidsWritten { get; init; }
            = [];

        public List<int> TemplateFidsVerified { get; init; }
            = [];

        public bool Verified { get; init; }

        public string? Error { get; init; }
    }
}
