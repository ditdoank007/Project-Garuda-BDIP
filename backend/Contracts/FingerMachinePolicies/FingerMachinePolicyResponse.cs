namespace BDIP.Contracts.FingerMachinePolicies;

public sealed class FingerMachinePolicyResponse
{
    public Guid MachineId { get; set; }

    public string MachineCode { get; set; } = "";

    public string MachineName { get; set; } = "";

    public int CollectionIntervalMinutes { get; set; }

    public bool CollectionEnabled { get; set; }

    public int TimeSyncIntervalMinutes { get; set; }

    public bool TimeSyncEnabled { get; set; }

    public DateTime UpdatedAt { get; set; }
}
