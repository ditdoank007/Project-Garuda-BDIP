using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Attendance;

public sealed class PostgreSqlAttendanceMasterMachineService
    : IAttendanceMasterMachineService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlAttendanceMasterMachineService(
        IOptions<ApplicationDbOptions> options)
    {
        _options = options.Value;
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
            ApplicationName = "BDIP Attendance Master Machine"
        };

        return NpgsqlDataSource.Create(
            builder.ConnectionString);
    }

    public async Task<AttendanceMasterMachineResponse> GetAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dataSource =
            CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    fm.id,
                    fm.code,
                    fm.name
                FROM public.attendance_settings s
                INNER JOIN public.finger_machines fm
                    ON fm.id = s.master_machine_id
                LIMIT 1;
                """);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "Master mesin finger belum dikonfigurasi.");
        }

        return new AttendanceMasterMachineResponse
        {
            MachineId = reader.GetGuid(0).ToString(),
            MachineCode = reader.GetString(1),
            MachineName = reader.GetString(2)
        };
    }

    public async Task<AttendanceMasterMachineResponse> UpdateAsync(
        UpdateAttendanceMasterMachineRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.MachineId))
        {
            throw new InvalidOperationException(
                "MachineId wajib diisi.");
        }

        if (!Guid.TryParse(
                request.MachineId,
                out var machineId))
        {
            throw new InvalidOperationException(
                "MachineId tidak valid.");
        }

        await using var dataSource =
            CreateDataSource();

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(
                cancellationToken);

        const string machineSql =
            """
            SELECT
                id,
                code,
                name
            FROM public.finger_machines
            WHERE id = @machine_id
              AND is_active = TRUE
            LIMIT 1;
            """;

        await using var machineCommand =
            new NpgsqlCommand(
                machineSql,
                connection,
                transaction);

        machineCommand.Parameters.AddWithValue(
            "machine_id",
            machineId);

        Guid selectedMachineId;
        string selectedMachineCode;
        string selectedMachineName;

        await using (var reader =
            await machineCommand.ExecuteReaderAsync(
                cancellationToken))
        {
            if (!await reader.ReadAsync(
                    cancellationToken))
            {
                throw new InvalidOperationException(
                    "Mesin finger tidak ditemukan atau tidak aktif.");
            }

            selectedMachineId =
                reader.GetGuid(0);

            selectedMachineCode =
                reader.GetString(1);

            selectedMachineName =
                reader.GetString(2);
        }

        const string upsertSql =
            """
            INSERT INTO public.attendance_settings
                (
                    id,
                    master_machine_id,
                    updated_at
                )
            VALUES
                (
                    gen_random_uuid(),
                    @machine_id,
                    NOW()
                )
            ON CONFLICT ((TRUE))
            DO UPDATE SET
                master_machine_id =
                    EXCLUDED.master_machine_id,
                updated_at = NOW();
            """;

        await using var upsertCommand =
            new NpgsqlCommand(
                upsertSql,
                connection,
                transaction);

        upsertCommand.Parameters.AddWithValue(
            "machine_id",
            selectedMachineId);

        await upsertCommand.ExecuteNonQueryAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return new AttendanceMasterMachineResponse
        {
            MachineId =
                selectedMachineId.ToString(),

            MachineCode =
                selectedMachineCode,

            MachineName =
                selectedMachineName
        };
    }
}
