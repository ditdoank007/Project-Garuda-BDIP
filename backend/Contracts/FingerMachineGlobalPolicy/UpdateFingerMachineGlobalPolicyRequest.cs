namespace BDIP.Contracts.FingerMachineGlobalPolicy;

public sealed class UpdateFingerMachineGlobalPolicyRequest
{
    public bool ClearEnabled { get; set; }

    public TimeSpan ClearTime { get; set; }
}
