using System.Net.Http.Json;
using System.Text.Json;

using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlAttendanceReconciliationService
    : IAttendanceReconciliationService
{
    private readonly ApplicationDbOptions _options;
    private readonly HttpClient _httpClient;

    public PostgreSqlAttendanceReconciliationService(
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
            ApplicationName = "BDIP Attendance Reconciliation"
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task<object> PreviewAsync(
        string machineCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
        {
            throw new ArgumentException("MachineCode wajib diisi.");
        }

        await using var dataSource = CreateDataSource();

        string actualMachineCode;
        string machineName;
        string ipAddress;
        int port;

        await using (var command = dataSource.CreateCommand())
        {
            command.CommandText = """
                SELECT
                    code,
                    name,
                    ip_address,
                    port
                FROM public.finger_machines
                WHERE LOWER(code) = LOWER(@code)
                  AND is_active = TRUE
                LIMIT 1;
                """;

            command.Parameters.AddWithValue(
                "code",
                machineCode.Trim());

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new KeyNotFoundException(
                    $"Finger machine '{machineCode}' tidak ditemukan.");
            }

            actualMachineCode = reader.GetString(0);
            machineName = reader.GetString(1);
            ipAddress = reader.GetString(2);
            port = reader.GetInt32(3);
        }

        var snapshot = await ReadMachineSnapshotAsync(
            ipAddress,
            port,
            actualMachineCode,
            cancellationToken);

        var result = await BuildComparisonAsync(
            dataSource,
            actualMachineCode,
            machineName,
            snapshot,
            cancellationToken);

        return result;
    }

    private async Task<JsonElement> ReadMachineSnapshotAsync(
        string ipAddress,
        int port,
        string machineCode,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "http://192.168.100.129:8090/attendance/snapshot",
            new
            {
                ip = ipAddress,
                port
            },
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Gagal membaca snapshot machine '{machineCode}'. " +
                $"HTTP {(int)response.StatusCode}: {body}");
        }

        using var document = JsonDocument.Parse(body);

        var root = document.RootElement.Clone();

        if (!root.TryGetProperty("success", out var success) ||
            !success.GetBoolean())
        {
            throw new InvalidOperationException(
                $"Collector mengembalikan snapshot gagal untuk machine '{machineCode}'.");
        }

        return root;
    }

    private static async Task<AttendanceReconciliationResponse>
        BuildComparisonAsync(
            NpgsqlDataSource dataSource,
            string machineCode,
            string machineName,
            JsonElement snapshot,
            CancellationToken cancellationToken)
    {
        var result = new AttendanceReconciliationResponse
        {
            Success = true,
            MachineCode = machineCode,
            MachineName = machineName
        };

        var machineUsers = new Dictionary<string, SnapshotUser>(
            StringComparer.OrdinalIgnoreCase);

        if (snapshot.TryGetProperty("users", out var usersElement))
        {
            foreach (var item in usersElement.EnumerateArray())
            {
                var fingerId =
                    item.TryGetProperty("userId", out var userId)
                        ? userId.GetString() ?? string.Empty
                        : string.Empty;

                if (string.IsNullOrWhiteSpace(fingerId))
                {
                    continue;
                }

                var templates = new Dictionary<int, string>();

                if (item.TryGetProperty("fingers", out var fingers))
                {
                    foreach (var finger in fingers.EnumerateArray())
                    {
                        if (!finger.TryGetProperty("fid", out var fidElement))
                        {
                            continue;
                        }

                        var fid = fidElement.GetInt32();

                        var template =
                            finger.TryGetProperty("template", out var templateElement)
                                ? templateElement.GetString() ?? string.Empty
                                : string.Empty;

                        templates[fid] = template;
                    }
                }

                machineUsers[fingerId] = new SnapshotUser
                {
                    FingerId = fingerId,
                    Name =
                        item.TryGetProperty("name", out var name)
                            ? name.GetString() ?? string.Empty
                            : string.Empty,
                    Enabled = true,
                    Templates = templates
                };
            }
        }

        var masterUsers = new Dictionary<string, MasterUser>(
            StringComparer.OrdinalIgnoreCase);

        await using (var command = dataSource.CreateCommand())
        {
            command.CommandText = """
                SELECT
                    u.id,
                    u.finger_id,
                    u.full_name,
                    u.enabled
                FROM public.users u
                WHERE u.finger_id IS NOT NULL
                  AND TRIM(u.finger_id) <> '';
                """;

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var fingerId = reader.GetString(1).Trim();

                masterUsers[fingerId] = new MasterUser
                {
                    UserId = reader.GetGuid(0),
                    FingerId = fingerId,
                    FullName = reader.IsDBNull(2)
                        ? string.Empty
                        : reader.GetString(2),
                    Enabled = reader.GetBoolean(3)
                };
            }
        }

        var masterTemplates =
            new Dictionary<string, Dictionary<int, string>>(
                StringComparer.OrdinalIgnoreCase);

        await using (var command = dataSource.CreateCommand())
        {
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
                  AND t.valid = TRUE;
                """;

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var fingerId = reader.GetString(0).Trim();
                var fid = reader.GetInt16(1);
                var template = reader.GetString(2);

                if (!masterTemplates.TryGetValue(
                    fingerId,
                    out var templates))
                {
                    templates = [];
                    masterTemplates[fingerId] = templates;
                }

                templates[fid] = template;
            }
        }

        result.MasterUserCount = masterUsers.Count;
        result.MachineUserCount = machineUsers.Count;

        foreach (var master in masterUsers.Values)
        {
            masterTemplates.TryGetValue(
                master.FingerId,
                out var masterFingers);

            masterFingers ??= [];

            if (!machineUsers.TryGetValue(
                master.FingerId,
                out var machine))
            {
                result.UserMissingCount++;

                result.Users.Add(new AttendanceReconciliationUser
                {
                    FingerId = master.FingerId,
                    FullName = master.FullName,
                    Status = "USER_MISSING",
                    MasterEnabled = master.Enabled,
                    MachineRegistered = false,
                    MachineEnabled = false,
                    MasterFingerprintCount = masterFingers.Count,
                    MachineFingerprintCount = 0,
                    MissingFids = masterFingers.Keys.Order().ToList()
                });

                continue;
            }

            var missing = masterFingers
                .Where(x =>
                    !machine.Templates.ContainsKey(x.Key))
                .Select(x => x.Key)
                .Order()
                .ToList();

            var different = masterFingers
                .Where(x =>
                    machine.Templates.TryGetValue(
                        x.Key,
                        out var machineTemplate) &&
                    !string.Equals(
                        x.Value,
                        machineTemplate,
                        StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Key)
                .Order()
                .ToList();

            var status =
                missing.Count > 0
                    ? "TEMPLATE_MISSING"
                    : different.Count > 0
                        ? "TEMPLATE_DIFFERENT"
                        : "MATCH";

            if (status == "MATCH")
            {
                result.MatchUserCount++;
            }
            else if (status == "TEMPLATE_MISSING")
            {
                result.TemplateMissingCount++;
            }
            else
            {
                result.TemplateDifferentCount++;
            }

            result.Users.Add(new AttendanceReconciliationUser
            {
                FingerId = master.FingerId,
                FullName = master.FullName,
                Status = status,
                MasterEnabled = master.Enabled,
                MachineRegistered = true,
                MachineEnabled = machine.Enabled,
                MasterFingerprintCount = masterFingers.Count,
                MachineFingerprintCount = machine.Templates.Count,
                MissingFids = missing,
                DifferentFids = different
            });
        }

        foreach (var machine in machineUsers.Values)
        {
            if (masterUsers.ContainsKey(machine.FingerId))
            {
                continue;
            }

            result.ExtraOnMachineCount++;

            result.Users.Add(new AttendanceReconciliationUser
            {
                FingerId = machine.FingerId,
                FullName = machine.Name,
                Status = "EXTRA_ON_MACHINE",
                MasterEnabled = false,
                MachineRegistered = true,
                MachineEnabled = machine.Enabled,
                MasterFingerprintCount = 0,
                MachineFingerprintCount = machine.Templates.Count
            });
        }

        return result;
    }

    private sealed class MasterUser
    {
        public Guid UserId { get; init; }
        public string FingerId { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public bool Enabled { get; init; }
    }

    private sealed class SnapshotUser
    {
        public string FingerId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public bool Enabled { get; init; }
        public Dictionary<int, string> Templates { get; init; } = [];
    }
}
