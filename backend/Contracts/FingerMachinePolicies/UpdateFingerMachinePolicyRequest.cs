namespace BDIP.Contracts.FingerMachinePolicies;

public sealed class UpdateFingerMachinePolicyRequest
{
    public int CollectionIntervalMinutes { get; set; } = 360;

    public bool CollectionEnabled { get; set; } = true;

    public int TimeSyncIntervalMinutes { get; set; } = 5;

    public bool TimeSyncEnabled { get; set; } = true;
}
