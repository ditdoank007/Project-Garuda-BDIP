namespace BDIP.Contracts.FingerMachineRuntime;

public sealed class FingerMachineRuntimeRequest
{
    public bool IsOnline { get; set; }

    public string? SerialNumber { get; set; }

    public string? DeviceName { get; set; }

    public DateTime? ObservedAt { get; set; }
}

public sealed class FingerMachinePullResultRequest
{
    public int ReadCount { get; set; }

    public int InsertedCount { get; set; }

    public int SkippedCount { get; set; }

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? ExecutedAt { get; set; }
}

public sealed class FingerMachineSyncResultRequest
{
    public DateTime? DeviceTimeBefore { get; set; }

    public DateTime? ServerTime { get; set; }

    public DateTime? DeviceTimeAfter { get; set; }

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? ExecutedAt { get; set; }
}

public sealed class FingerMachineClearResultRequest
{
    public int? ClearedCount { get; set; }

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? ExecutedAt { get; set; }
}
