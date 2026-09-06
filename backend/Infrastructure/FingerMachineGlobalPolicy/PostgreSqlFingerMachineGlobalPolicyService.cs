using BDIP.Application.FingerMachineGlobalPolicy;
using BDIP.Contracts.FingerMachineGlobalPolicy;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.FingerMachineGlobalPolicy;

public sealed class PostgreSqlFingerMachineGlobalPolicyService
    : IFingerMachineGlobalPolicyService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlFingerMachineGlobalPolicyService(
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

    public async Task<FingerMachineGlobalPolicyResponse>
        GetAsync()
    {
        await using var dataSource =
            CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    clear_enabled,
                    clear_time
                FROM public.finger_machine_global_policy
                LIMIT 1;
                """);

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException(
                "Global finger machine policy not found.");
        }

        return new FingerMachineGlobalPolicyResponse
        {
            ClearEnabled =
                reader.GetBoolean(0),

            ClearTime =
                reader.GetTimeSpan(1)
        };
    }

    public async Task<FingerMachineGlobalPolicyResponse>
        UpdateAsync(
            UpdateFingerMachineGlobalPolicyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ClearTime < TimeSpan.Zero ||
            request.ClearTime >= TimeSpan.FromDays(1))
        {
            throw new InvalidOperationException(
                "Clear time must be between 00:00 and 23:59.");
        }

        await using var dataSource =
            CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                UPDATE public.finger_machine_global_policy
                SET
                    clear_enabled = @clear_enabled,
                    clear_time = @clear_time,
                    updated_at = NOW()
                WHERE id = (
                    SELECT id
                    FROM public.finger_machine_global_policy
                    LIMIT 1
                );
                """);

        command.Parameters.AddWithValue(
            "clear_enabled",
            request.ClearEnabled);

        command.Parameters.AddWithValue(
            "clear_time",
            request.ClearTime);

        await command.ExecuteNonQueryAsync();

        return await GetAsync();
    }
}
