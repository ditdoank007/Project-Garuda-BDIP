using System.Net.Http.Json;
using System.Text.Json;

using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlAttendanceMachineUserService
    : IAttendanceMachineUserService
{
    private readonly ApplicationDbOptions _options;
    private readonly HttpClient _httpClient;

    public PostgreSqlAttendanceMachineUserService(
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
            ApplicationName = "BDIP Attendance Machine User"
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task<AttendanceMachineUserEnabledResponse> SetEnabledAsync(
        AttendanceMachineUserEnabledRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.MachineCode))
        {
            throw new ArgumentException("MachineCode wajib diisi.");
        }

        if (request.DeviceUid < 0)
        {
            throw new ArgumentException("DeviceUid harus >= 0.");
        }

        await using var dataSource = CreateDataSource();

        string machineCode;
        string ipAddress;
        int port;

        await using (var machineCommand = dataSource.CreateCommand())
        {
            machineCommand.CommandText = """
                SELECT
                    code,
                    ip_address,
                    port
                FROM public.finger_machines
                WHERE LOWER(code) = LOWER(@code)
                  AND is_active = TRUE
                LIMIT 1;
                """;

            machineCommand.Parameters.AddWithValue(
                "code",
                request.MachineCode.Trim());

            await using var reader =
                await machineCommand.ExecuteReaderAsync(
                    cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new KeyNotFoundException(
                    $"Finger machine '{request.MachineCode}' tidak ditemukan.");
            }

            machineCode = reader.GetString(0);

            ipAddress =
                reader.IsDBNull(1)
                    ? string.Empty
                    : reader.GetString(1);

            port = reader.GetInt32(2);
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            throw new InvalidOperationException(
                $"Finger machine '{machineCode}' tidak memiliki IP address.");
        }

        // Pastikan pasangan machine + device UID memang terdaftar
        // di Master Attendance sebelum menyentuh device.
        Guid machineUserId;

        await using (var machineUserCommand = dataSource.CreateCommand())
        {
            machineUserCommand.CommandText = """
                SELECT id
                FROM public.attendance_finger_machine_users
                WHERE finger_machine_id = (
                    SELECT id
                    FROM public.finger_machines
                    WHERE LOWER(code) = LOWER(@machine_code)
                      AND is_active = TRUE
                    LIMIT 1
                )
                  AND device_uid = @device_uid
                LIMIT 1;
                """;

            machineUserCommand.Parameters.AddWithValue(
                "machine_code",
                machineCode);

            machineUserCommand.Parameters.AddWithValue(
                "device_uid",
                request.DeviceUid);

            var result =
                await machineUserCommand.ExecuteScalarAsync(
                    cancellationToken);

            if (result is null)
            {
                throw new KeyNotFoundException(
                    $"Device UID {request.DeviceUid} tidak terdaftar pada machine '{machineCode}'.");
            }

            machineUserId = (Guid)result;
        }

        var payload = new
        {
            ip = ipAddress,
            port = port,
            uid = request.DeviceUid,
            enabled = request.Enabled
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "http://192.168.100.129:8090/attendance/user-enabled",
                payload,
                cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        AttendanceMachineUserEnabledDeviceResponse? deviceResult = null;

        try
        {
            deviceResult =
                JsonSerializer.Deserialize<AttendanceMachineUserEnabledDeviceResponse>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
        }
        catch (JsonException)
        {
            // Ditangani sebagai response collector yang tidak valid.
        }

        if (!response.IsSuccessStatusCode ||
            deviceResult is null ||
            !deviceResult.Success ||
            !deviceResult.Found ||
            deviceResult.Enabled != request.Enabled)
        {
            throw new InvalidOperationException(
                deviceResult?.Message
                ?? deviceResult?.Error
                ?? $"Attendance collector gagal mengubah status user. HTTP {(int)response.StatusCode}.");
        }

        // Device sudah berhasil diverifikasi.
        // BARU sekarang database BDIP diperbarui.
        await using (var updateCommand = dataSource.CreateCommand())
        {
            updateCommand.CommandText = """
                UPDATE public.attendance_finger_machine_users
                SET
                    enabled = @enabled,
                    updated_at = NOW()
                WHERE id = @id;
                """;

            updateCommand.Parameters.AddWithValue(
                "enabled",
                request.Enabled);

            updateCommand.Parameters.AddWithValue(
                "id",
                machineUserId);

            var affected =
                await updateCommand.ExecuteNonQueryAsync(
                    cancellationToken);

            if (affected != 1)
            {
                throw new InvalidOperationException(
                    "Status mesin berhasil diubah, tetapi database BDIP gagal diperbarui.");
            }
        }

        return new AttendanceMachineUserEnabledResponse
        {
            Success = true,
            MachineCode = machineCode,
            DeviceUid = request.DeviceUid,
            RequestedEnabled = request.Enabled,
            DeviceEnabled = deviceResult.Enabled,
            DatabaseUpdated = true,
            Message =
                request.Enabled
                    ? "User berhasil diaktifkan pada Finger Machine dan BDIP."
                    : "User berhasil dinonaktifkan pada Finger Machine dan BDIP."
        };
    }

    private sealed class AttendanceMachineUserEnabledDeviceResponse
    {
        public bool Success { get; set; }

        public bool Found { get; set; }

        public bool Changed { get; set; }

        public int Uid { get; set; }

        public bool Enabled { get; set; }

        public bool PreviousEnabled { get; set; }

        public string? Message { get; set; }

        public string? Error { get; set; }
    }
}
