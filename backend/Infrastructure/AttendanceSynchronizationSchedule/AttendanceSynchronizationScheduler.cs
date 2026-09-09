using BDIP.Application.Attendance;
using BDIP.Application.AttendanceSynchronizationSchedule;
using BDIP.Contracts.Attendance;
using BDIP.Contracts.AttendanceSynchronizationSchedule;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BDIP.Infrastructure.AttendanceSynchronizationSchedule;

public sealed class AttendanceSynchronizationScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AttendanceSynchronizationScheduler> _logger;

    private DateTimeOffset? _lastRunKey;

    public AttendanceSynchronizationScheduler(
        IServiceScopeFactory scopeFactory,
        ILogger<AttendanceSynchronizationScheduler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Attendance synchronization scheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndRunAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Attendance synchronization scheduler error.");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation(
            "Attendance synchronization scheduler stopped.");
    }

    private async Task CheckAndRunAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var scheduleService =
            scope.ServiceProvider
                .GetRequiredService<
                    IAttendanceSynchronizationScheduleService>();

        var schedule =
            await scheduleService.GetAsync(cancellationToken);

        if (!schedule.IsEnabled)
        {
            return;
        }

        var now = DateTimeOffset.Now;

        if (!IsDue(schedule, now))
        {
            return;
        }

        var runKey = BuildRunKey(schedule, now);

        if (_lastRunKey == runKey)
        {
            return;
        }

        _lastRunKey = runKey;

        _logger.LogInformation(
            "Starting scheduled Attendance synchronization. " +
            "Frequency={Frequency}, Time={SyncTime}",
            schedule.Frequency,
            schedule.SyncTime);

        var startedAt = DateTimeOffset.Now;

        try
        {
            var synchronizationService =
                scope.ServiceProvider
                    .GetRequiredService<
                        IAttendanceSynchronizationService>();

            var result =
                await synchronizationService.SyncNowAsync(
                    null,
                    cancellationToken);

            await UpdateExecutionStateAsync(
                schedule.Id,
                startedAt,
                DateTimeOffset.Now,
                result.Success,
                BuildResultMessage(result),
                cancellationToken);

            _logger.LogInformation(
                "Scheduled Attendance synchronization completed. " +
                "Success={Success}",
                result.Success);
        }
        catch (Exception exception)
        {
            await UpdateExecutionStateAsync(
                schedule.Id,
                startedAt,
                DateTimeOffset.Now,
                false,
                exception.Message,
                cancellationToken);

            _logger.LogError(
                exception,
                "Scheduled Attendance synchronization failed.");
        }
    }

    private static bool IsDue(
        AttendanceSynchronizationScheduleResponse schedule,
        DateTimeOffset now)
    {
        if (!TimeSpan.TryParse(
                schedule.SyncTime,
                out var syncTime))
        {
            return false;
        }

        if (now.TimeOfDay < syncTime)
        {
            return false;
        }

        return schedule.Frequency switch
        {
            "DAILY" =>
                true,

            "WEEKLY" =>
                schedule.Weekday.HasValue &&
                ToIsoWeekday(now.DayOfWeek)
                    == schedule.Weekday.Value,

            "MONTHLY" =>
                schedule.DayOfMonth.HasValue &&
                now.Day == schedule.DayOfMonth.Value,

            _ =>
                false
        };
    }

    private static int ToIsoWeekday(
        DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            DayOfWeek.Saturday => 6,
            DayOfWeek.Sunday => 7,
            _ => 7
        };
    }

    private static DateTimeOffset BuildRunKey(
        AttendanceSynchronizationScheduleResponse schedule,
        DateTimeOffset now)
    {
        return schedule.Frequency switch
        {
            "DAILY" =>
                new DateTimeOffset(
                    now.Year,
                    now.Month,
                    now.Day,
                    0,
                    0,
                    0,
                    now.Offset),

            "WEEKLY" =>
                new DateTimeOffset(
                    now.Year,
                    now.Month,
                    now.Day,
                    0,
                    0,
                    0,
                    now.Offset),

            "MONTHLY" =>
                new DateTimeOffset(
                    now.Year,
                    now.Month,
                    1,
                    0,
                    0,
                    0,
                    now.Offset),

            _ => now
        };
    }

    private async Task UpdateExecutionStateAsync(
        Guid scheduleId,
        DateTimeOffset startedAt,
        DateTimeOffset finishedAt,
        bool success,
        string message,
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var stateService =
            scope.ServiceProvider
                .GetRequiredService<
                    IAttendanceSynchronizationScheduleStateService>();

        await stateService.UpdateExecutionStateAsync(
            scheduleId,
            startedAt,
            finishedAt,
            success,
            message,
            cancellationToken);
    }

    private static string BuildResultMessage(
        AttendanceSynchronizationResponse result)
    {
        return
            $"Machines={result.MachineCount}, " +
            $"Success={result.SuccessMachineCount}, " +
            $"Pending={result.PendingMachineCount}, " +
            $"Failed={result.FailedMachineCount}.";
    }
}
