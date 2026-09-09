using System.Net.Http.Json;
using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using BDIP.Persistence.PostgreSQL;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlHrisStatusSyncService
    : IHrisStatusSyncService
{
    private readonly ApplicationDbOptions _options;
    private readonly HttpClient _httpClient;

    public PostgreSqlHrisStatusSyncService(
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
            ApplicationName = "BDIP HRIS Status Sync"
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task<AttendanceHrisStatusResponse> SyncStatusAsync(
        AttendanceHrisStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var fingerId = request.FingerId.Trim();

        if (string.IsNullOrWhiteSpace(fingerId))
        {
            throw new ArgumentException(
                "FingerId wajib diisi.",
                nameof(request.FingerId));
        }

        var isOut = string.Equals(
            request.IsKeluar?.Trim(),
            "Y",
            StringComparison.OrdinalIgnoreCase);

        await using var dataSource = CreateDataSource();

        Guid userId;
        bool currentEnabled;

        await using (var command = dataSource.CreateCommand(
            """
            SELECT id, enabled
            FROM public.users
            WHERE finger_id = @finger_id
            ORDER BY created_at
            LIMIT 1;
            """))
        {
            command.Parameters.AddWithValue("finger_id", fingerId);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return new AttendanceHrisStatusResponse
                {
                    Success = true,
                    FingerId = fingerId,
                    HrisStatus = request.IsKeluar,
                    UserFound = false,
                    StatusChanged = false,
                    Message =
                        $"FingerID {fingerId} belum terdaftar di BDIP."
                };
            }

            userId = reader.GetGuid(0);
            currentEnabled = reader.GetBoolean(1);
        }

        // HRIS hanya boleh memaksa DISABLE.
        // Status aktif dari HRIS tidak melakukan auto-enable.
        if (!isOut)
        {
            return new AttendanceHrisStatusResponse
            {
                Success = true,
                FingerId = fingerId,
                HrisStatus = request.IsKeluar,
                UserFound = true,
                StatusChanged = false,
                Message = currentEnabled
                    ? $"FingerID {fingerId} aktif di BDIP. Tidak ada perubahan."
                    : $"FingerID {fingerId} aktif di HRIS, tetapi status disabled BDIP dipertahankan."
            };
        }

        var machines =
            new List<(Guid RowId, string MachineCode, string IpAddress, int Port, int DeviceUid)>();

        await using (var command = dataSource.CreateCommand(
            """
            SELECT
                afmu.id,
                fm.code,
                fm.ip_address,
                fm.port,
                afmu.device_uid
            FROM public.attendance_finger_machine_users afmu
            JOIN public.finger_machines fm
              ON fm.id = afmu.finger_machine_id
            WHERE afmu.user_id = @user_id
              AND fm.is_active = TRUE
            ORDER BY fm.code;
            """))
        {
            command.Parameters.AddWithValue("user_id", userId);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                machines.Add(
                    (
                        reader.GetGuid(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetInt32(3),
                        reader.GetInt32(4)
                    ));
            }
        }

        var updatedCount = 0;
        var failedMachines = new List<string>();

        foreach (var machine in machines)
        {
            try
            {
                var collectorResponse = await _httpClient.PostAsJsonAsync(
                    "http://192.168.100.129:8090/attendance/user-enabled",
                    new
                    {
                        ip = machine.IpAddress,
                        port = machine.Port,
                        uid = machine.DeviceUid,
                        enabled = false
                    },
                    cancellationToken);

                if (!collectorResponse.IsSuccessStatusCode)
                {
                    failedMachines.Add(
                        $"{machine.MachineCode} (HTTP {(int)collectorResponse.StatusCode})");
                    continue;
                }

                var result =
                    await collectorResponse.Content
                        .ReadFromJsonAsync<AttendanceCollectorUserEnabledResponse>(
                            cancellationToken);

                if (result is null ||
                    !result.Success ||
                    !result.Found ||
                    result.Enabled)
                {
                    failedMachines.Add(
                        $"{machine.MachineCode} (tidak mengonfirmasi disabled)");
                    continue;
                }

                await using var updateCommand = dataSource.CreateCommand(
                    """
                    UPDATE public.attendance_finger_machine_users
                    SET
                        enabled = FALSE,
                        updated_at = NOW()
                    WHERE id = @id;
                    """);

                updateCommand.Parameters.AddWithValue("id", machine.RowId);

                await updateCommand.ExecuteNonQueryAsync(cancellationToken);

                updatedCount++;
            }
            catch (Exception ex)
            {
                var reason = ex switch
                {
                    HttpRequestException =>
                        "tidak dapat dihubungi",
                    TaskCanceledException =>
                        "timeout/koneksi tidak tersedia",
                    _ =>
                        $"error: {ex.Message}"
                };

                failedMachines.Add(
                    $"{machine.MachineCode} ({reason})");
            }
        }

        await using (var userCommand = dataSource.CreateCommand(
            """
            UPDATE public.users
            SET
                enabled = FALSE,
                updated_at = NOW()
            WHERE id = @user_id;
            """))
        {
            userCommand.Parameters.AddWithValue("user_id", userId);

            await userCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        var message =
            $"FingerID {fingerId} dinonaktifkan di BDIP. " +
            $"{updatedCount} dari {machines.Count} mesin berhasil dinonaktifkan.";

        if (failedMachines.Count > 0)
        {
            message +=
                $" Mesin yang belum tersinkron: {string.Join(", ", failedMachines)}.";
        }

        return new AttendanceHrisStatusResponse
        {
            Success = true,
            FingerId = fingerId,
            HrisStatus = request.IsKeluar,
            UserFound = true,
            StatusChanged = currentEnabled,
            MachineCount = machines.Count,
            MachineUpdatedCount = updatedCount,
            Message = message
        };
    }

    private sealed class AttendanceCollectorUserEnabledResponse
    {
        public bool Success { get; set; }
        public bool Found { get; set; }
        public bool Enabled { get; set; }
    }
}
