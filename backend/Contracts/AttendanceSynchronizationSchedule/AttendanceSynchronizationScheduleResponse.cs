namespace BDIP.Contracts.AttendanceSynchronizationSchedule;

public sealed class AttendanceSynchronizationScheduleResponse
{
    public Guid Id { get; init; }

    public bool IsEnabled { get; init; }

    public string Frequency { get; init; } = "DAILY";

    public string SyncTime { get; init; } = "02:00";

    public int? Weekday { get; init; }

    public int? DayOfMonth { get; init; }

    public DateTimeOffset? LastStartedAt { get; init; }

    public DateTimeOffset? LastFinishedAt { get; init; }

    public bool? LastSuccess { get; init; }

    public string? LastMessage { get; init; }

    public DateTimeOffset? NextSyncAt { get; init; }
}
