using System.Net.Http.Json;

using BDIP.Application.Attendance;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlAttendanceMachineSnapshotService
    : IAttendanceMachineSnapshotService
{
    private readonly ApplicationDbOptions _options;
    private readonly HttpClient _httpClient;

    public PostgreSqlAttendanceMachineSnapshotService(
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
            ApplicationName = "BDIP Attendance Machine Snapshot"
        };

        return NpgsqlDataSource.Create(
            builder.ConnectionString);
    }

    public async Task<object> GetSnapshotAsync(
        string machineCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
        {
            throw new ArgumentException(
                "MachineCode wajib diisi.");
        }

        await using var dataSource = CreateDataSource();

        string actualMachineCode;
        string ipAddress;
        int port;

        await using (var command = dataSource.CreateCommand())
        {
            command.CommandText = """
                SELECT
                    code,
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
                await command.ExecuteReaderAsync(
                    cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new KeyNotFoundException(
                    $"Finger machine '{machineCode}' tidak ditemukan.");
            }

            actualMachineCode = reader.GetString(0);

            ipAddress =
                reader.IsDBNull(1)
                    ? string.Empty
                    : reader.GetString(1);

            port = reader.GetInt32(2);
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            throw new InvalidOperationException(
                $"Finger machine '{actualMachineCode}' tidak memiliki IP address.");
        }

        var payload = new
        {
            ip = ipAddress,
            port = port
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "http://192.168.100.129:8090/attendance/snapshot",
                payload,
                cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Attendance collector gagal membaca snapshot machine '{actualMachineCode}'. HTTP {(int)response.StatusCode}: {responseBody}");
        }

        return responseBody;
    }
}
