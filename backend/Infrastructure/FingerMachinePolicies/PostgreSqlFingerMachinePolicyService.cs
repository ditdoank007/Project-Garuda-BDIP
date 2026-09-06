using BDIP.Application.FingerMachinePolicies;
using BDIP.Contracts.FingerMachinePolicies;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.FingerMachinePolicies;

public sealed class PostgreSqlFingerMachinePolicyService
    : IFingerMachinePolicyService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlFingerMachinePolicyService(
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
            ApplicationName = "BDIP Backend"
        };

        return NpgsqlDataSource.Create(
            builder.ConnectionString);
    }

    public async Task<FingerMachinePolicyResponse?>
        GetByMachineCodeAsync(string machineCode)
    {
        var code = NormalizeCode(machineCode);

        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException(
                "Machine code is required.");

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    fm.id,
                    fm.code,
                    fm.name,

                    COALESCE(cp.interval_minutes, 360),
                    COALESCE(cp.is_enabled, TRUE),

                    COALESCE(tp.interval_minutes, 5),
                    COALESCE(tp.is_enabled, TRUE),

                    GREATEST(
                        COALESCE(cp.updated_at, fm.updated_at),
                        COALESCE(tp.updated_at, fm.updated_at)
                    ) AS updated_at

                FROM public.finger_machines fm

                LEFT JOIN
                    public.finger_machine_collection_policies cp
                    ON cp.finger_machine_id = fm.id

                LEFT JOIN
                    public.finger_machine_time_sync_policies tp
                    ON tp.finger_machine_id = fm.id

                WHERE LOWER(fm.code) = LOWER(@code);
                """
            );

        command.Parameters.AddWithValue("code", code);

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return Map(reader);
    }

    public async Task<FingerMachinePolicyResponse>
        UpsertAsync(
            string machineCode,
            UpdateFingerMachinePolicyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var code = NormalizeCode(machineCode);

        ValidateInterval(
            request.CollectionIntervalMinutes,
            "Collection interval");

        ValidateInterval(
            request.TimeSyncIntervalMinutes,
            "Time sync interval");

        await using var dataSource = CreateDataSource();

        await using var machineCommand =
            dataSource.CreateCommand(
                """
                SELECT id
                FROM public.finger_machines
                WHERE LOWER(code) = LOWER(@code);
                """
            );

        machineCommand.Parameters.AddWithValue(
            "code",
            code);

        var machineId =
            await machineCommand.ExecuteScalarAsync();

        if (machineId is null)
        {
            throw new InvalidOperationException(
                $"Machine '{code}' not found.");
        }

        var id = (Guid)machineId;
        var now = DateTime.UtcNow;

        await using var connection =
            await dataSource.OpenConnectionAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        await using var collectionCommand =
            connection.CreateCommand();

        collectionCommand.CommandText =
            """
            INSERT INTO
                public.finger_machine_collection_policies
            (
                id,
                finger_machine_id,
                interval_minutes,
                is_enabled,
                created_at,
                updated_at
            )
            VALUES
            (
                @id,
                @machine_id,
                @interval,
                @enabled,
                @created_at,
                @updated_at
            )
            ON CONFLICT (finger_machine_id)
            DO UPDATE SET
                interval_minutes =
                    EXCLUDED.interval_minutes,
                is_enabled =
                    EXCLUDED.is_enabled,
                updated_at =
                    EXCLUDED.updated_at;
            """;

        collectionCommand.Transaction = transaction;

        collectionCommand.Parameters.AddWithValue(
            "id",
            Guid.NewGuid());

        collectionCommand.Parameters.AddWithValue(
            "machine_id",
            id);

        collectionCommand.Parameters.AddWithValue(
            "interval",
            request.CollectionIntervalMinutes);

        collectionCommand.Parameters.AddWithValue(
            "enabled",
            request.CollectionEnabled);

        collectionCommand.Parameters.AddWithValue(
            "created_at",
            now);

        collectionCommand.Parameters.AddWithValue(
            "updated_at",
            now);

        await collectionCommand.ExecuteNonQueryAsync();

        await using var timeSyncCommand =
            connection.CreateCommand();

        timeSyncCommand.CommandText =
            """
            INSERT INTO
                public.finger_machine_time_sync_policies
            (
                id,
                finger_machine_id,
                interval_minutes,
                is_enabled,
                created_at,
                updated_at
            )
            VALUES
            (
                @id,
                @machine_id,
                @interval,
                @enabled,
                @created_at,
                @updated_at
            )
            ON CONFLICT (finger_machine_id)
            DO UPDATE SET
                interval_minutes =
                    EXCLUDED.interval_minutes,
                is_enabled =
                    EXCLUDED.is_enabled,
                updated_at =
                    EXCLUDED.updated_at;
            """;

        timeSyncCommand.Transaction = transaction;

        timeSyncCommand.Parameters.AddWithValue(
            "id",
            Guid.NewGuid());

        timeSyncCommand.Parameters.AddWithValue(
            "machine_id",
            id);

        timeSyncCommand.Parameters.AddWithValue(
            "interval",
            request.TimeSyncIntervalMinutes);

        timeSyncCommand.Parameters.AddWithValue(
            "enabled",
            request.TimeSyncEnabled);

        timeSyncCommand.Parameters.AddWithValue(
            "created_at",
            now);

        timeSyncCommand.Parameters.AddWithValue(
            "updated_at",
            now);

        await timeSyncCommand.ExecuteNonQueryAsync();

        await transaction.CommitAsync();

        return await GetByMachineCodeAsync(code)
            ?? throw new InvalidOperationException(
                "Policy was not found after update.");
    }

    private static FingerMachinePolicyResponse Map(
        NpgsqlDataReader reader)
    {
        return new FingerMachinePolicyResponse
        {
            MachineId = reader.GetGuid(0),
            MachineCode = reader.GetString(1),
            MachineName = reader.GetString(2),
            CollectionIntervalMinutes =
                reader.GetInt32(3),
            CollectionEnabled =
                reader.GetBoolean(4),
            TimeSyncIntervalMinutes =
                reader.GetInt32(5),
            TimeSyncEnabled =
                reader.GetBoolean(6),
            UpdatedAt =
                reader.GetDateTime(7)
        };
    }

    private static string NormalizeCode(
        string value)
        => value.Trim();

    private static void ValidateInterval(
        int value,
        string name)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException(
                $"{name} must be greater than zero.");
        }
    }
}
