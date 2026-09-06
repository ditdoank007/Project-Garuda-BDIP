namespace BDIP.Contracts.FingerMachineGlobalPolicy;

public sealed class FingerMachineGlobalPolicyResponse
{
    public bool ClearEnabled { get; set; }

    public TimeSpan ClearTime { get; set; }
}
