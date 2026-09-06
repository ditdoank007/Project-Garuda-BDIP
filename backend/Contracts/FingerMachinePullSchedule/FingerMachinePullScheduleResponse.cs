namespace BDIP.Contracts.FingerMachinePullSchedule;

public sealed class FingerMachinePullScheduleResponse
{
    public Guid Id { get; set; }

    public string PullTime { get; set; } = "";

    public bool IsActive { get; set; }
}
