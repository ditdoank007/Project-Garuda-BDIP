namespace BDIP.Contracts.AttendanceSynchronizationSchedule;

public sealed class UpdateAttendanceSynchronizationScheduleRequest
{
    public bool IsEnabled { get; init; }

    public string Frequency { get; init; } = "DAILY";

    public string SyncTime { get; init; } = "02:00";

    public int? Weekday { get; init; }

    public int? DayOfMonth { get; init; }
}
