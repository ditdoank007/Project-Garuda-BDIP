using System.Net.Http.Json;
using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using BDIP.Persistence.PostgreSQL;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlAttendanceGlobalUserStatusService
    : IAttendanceGlobalUserStatusService
{
    private readonly ApplicationDbOptions _options;
    private readonly HttpClient _httpClient;

    public PostgreSqlAttendanceGlobalUserStatusService(
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
            ApplicationName = "BDIP Attendance Global Status"
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task<AttendanceGlobalUserStatusResponse> SetStatusAsync(
        AttendanceGlobalUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId wajib diisi.",
                nameof(request.UserId));
        }

        await using var dataSource = CreateDataSource();

        var machines = new List<(Guid RowId, string MachineCode, string IpAddress, int Port, int DeviceUid)>();

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
            command.Parameters.AddWithValue("user_id", request.UserId);

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

        foreach (var machine in machines)
        {
            var collectorResponse = await _httpClient.PostAsJsonAsync(
                "http://192.168.100.129:8090/attendance/user-enabled",
                new
                {
                    ip = machine.IpAddress,
                    port = machine.Port,
                    uid = machine.DeviceUid,
                    enabled = request.Enabled
                },
                cancellationToken);

            if (!collectorResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Gagal mengubah status FingerID pada {machine.MachineCode}: " +
                    $"HTTP {(int)collectorResponse.StatusCode}.");
            }

            var result =
                await collectorResponse.Content
                    .ReadFromJsonAsync<AttendanceCollectorUserEnabledResponse>(
                        cancellationToken);

            if (result is null ||
                !result.Success ||
                !result.Found ||
                result.Enabled != request.Enabled)
            {
                throw new InvalidOperationException(
                    $"Finger Machine {machine.MachineCode} tidak mengonfirmasi " +
                    $"status yang diminta.");
            }

            await using var updateCommand = dataSource.CreateCommand(
                """
                UPDATE public.attendance_finger_machine_users
                SET
                    enabled = @enabled,
                    updated_at = NOW()
                WHERE id = @id;
                """);

            updateCommand.Parameters.AddWithValue(
                "enabled",
                request.Enabled);

            updateCommand.Parameters.AddWithValue(
                "id",
                machine.RowId);

            await updateCommand.ExecuteNonQueryAsync(cancellationToken);

            updatedCount++;
        }

        await using (var userCommand = dataSource.CreateCommand(
            """
            UPDATE public.users
            SET
                enabled = @enabled,
                updated_at = NOW()
            WHERE id = @user_id;
            """))
        {
            userCommand.Parameters.AddWithValue(
                "enabled",
                request.Enabled);

            userCommand.Parameters.AddWithValue(
                "user_id",
                request.UserId);

            var rows =
                await userCommand.ExecuteNonQueryAsync(cancellationToken);

            if (rows == 0)
            {
                throw new KeyNotFoundException(
                    $"User {request.UserId} tidak ditemukan.");
            }
        }

        return new AttendanceGlobalUserStatusResponse
        {
            Success = true,
            UserId = request.UserId,
            RequestedEnabled = request.Enabled,
            DatabaseUpdated = true,
            MachineCount = machines.Count,
            MachineUpdatedCount = updatedCount,
            Message = request.Enabled
                ? "User berhasil diaktifkan pada BDIP dan seluruh Finger Machine."
                : "User berhasil dinonaktifkan pada BDIP dan seluruh Finger Machine."
        };
    }

    private sealed class AttendanceCollectorUserEnabledResponse
    {
        public bool Success { get; set; }
        public bool Found { get; set; }
        public bool Enabled { get; set; }
    }
}
