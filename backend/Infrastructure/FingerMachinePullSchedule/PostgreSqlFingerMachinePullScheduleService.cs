using BDIP.Application.FingerMachinePullSchedule;
using BDIP.Contracts.FingerMachinePullSchedule;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.FingerMachinePullSchedule;

public sealed class PostgreSqlFingerMachinePullScheduleService
    : IFingerMachinePullScheduleService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlFingerMachinePullScheduleService(
        IOptions<ApplicationDbOptions> options)
    {
        _options = options.Value;
    }

    private NpgsqlDataSource CreateDataSource()
    {
        var builder =
            new NpgsqlConnectionStringBuilder
            {
                Host = _options.Host,
                Port = _options.Port,
                Database = _options.Database,
                Username = _options.Username,
                Password = _options.Password,
                SslMode = SslMode.Disable
            };

        return NpgsqlDataSource.Create(
            builder.ConnectionString);
    }


    public async Task<IEnumerable<FingerMachinePullScheduleResponse>>
        GetAllAsync()
    {
        await using var dataSource =
            CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    pull_time,
                    is_active
                FROM public.finger_machine_pull_schedule
                ORDER BY pull_time;
                """
            );

        await using var reader =
            await command.ExecuteReaderAsync();

        var result =
            new List<FingerMachinePullScheduleResponse>();

        while (await reader.ReadAsync())
        {
            result.Add(
                new FingerMachinePullScheduleResponse
                {
                    Id =
                        reader.GetGuid(0),

                    PullTime =
                        reader.GetTimeSpan(1)
                            .ToString(@"hh\:mm"),

                    IsActive =
                        reader.GetBoolean(2)
                });
        }

        return result;
    }


    public async Task<IEnumerable<FingerMachinePullScheduleResponse>>
        UpdateAsync(
            UpdateFingerMachinePullScheduleRequest request)
    {
        await using var dataSource =
            CreateDataSource();

        await using var connection =
            await dataSource.OpenConnectionAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();


        await using (
            var deactivateCommand =
                connection.CreateCommand())
        {
            deactivateCommand.Transaction =
                transaction;

            deactivateCommand.CommandText =
                """
                UPDATE public.finger_machine_pull_schedule
                SET
                    is_active = FALSE,
                    updated_at = NOW();
                """;

            await deactivateCommand.ExecuteNonQueryAsync();
        }


        foreach (var time in request.PullTimes)
        {
            await using var activateCommand =
                connection.CreateCommand();

            activateCommand.Transaction =
                transaction;

            activateCommand.CommandText =
                """
                UPDATE public.finger_machine_pull_schedule
                SET
                    is_active = TRUE,
                    updated_at = NOW()
                WHERE pull_time = @pull_time;
                """;

            activateCommand.Parameters.AddWithValue(
                "pull_time",
                TimeSpan.Parse(time));

            await activateCommand.ExecuteNonQueryAsync();
        }



        await transaction.CommitAsync();

        return await GetAllAsync();
    }
}
