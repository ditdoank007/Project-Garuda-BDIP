namespace BDIP.Application.AttendanceSynchronizationSchedule;

public interface IAttendanceSynchronizationScheduleStateService
{
    Task UpdateExecutionStateAsync(
        Guid scheduleId,
        DateTimeOffset startedAt,
        DateTimeOffset finishedAt,
        bool success,
        string message,
        CancellationToken cancellationToken = default);
}
