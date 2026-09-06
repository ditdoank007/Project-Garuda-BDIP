namespace BDIP.Contracts.FingerMachineRuntime;

public sealed class FingerMachineRuntimeResponse
{
    public string MachineCode { get; set; } = "";

    public string MachineName { get; set; } = "";

    public bool IsOnline { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public DateTime? LastPullAt { get; set; }

    public DateTime? LastTimeSyncAt { get; set; }

    public DateTime? LastClearAt { get; set; }

    public string? LastError { get; set; }
}
