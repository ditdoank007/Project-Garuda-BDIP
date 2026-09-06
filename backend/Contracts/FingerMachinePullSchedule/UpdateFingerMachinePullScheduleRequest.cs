namespace BDIP.Contracts.FingerMachinePullSchedule;

public sealed class UpdateFingerMachinePullScheduleRequest
{
    public List<string> PullTimes { get; set; } = new();
}
