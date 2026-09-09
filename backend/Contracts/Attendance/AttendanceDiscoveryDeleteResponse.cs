namespace BDIP.Contracts.Attendance;

public sealed class AttendanceDiscoveryDeleteResponse
{
    public bool Success { get; set; }

    public string MachineCode { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    public int RequestedCount { get; set; }

    public int DeletedCount { get; set; }

    public int FailedCount { get; set; }

    public int SkippedCount { get; set; }

    public List<AttendanceDiscoveryDeleteResult> Results { get; set; } = new();
}

public sealed class AttendanceDiscoveryDeleteResult
{
    public string FingerId { get; set; } = string.Empty;

    public int DeviceUid { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public bool Success { get; set; }

    public bool Deleted { get; set; }

    public bool Verified { get; set; }

    public string Error { get; set; } = string.Empty;
}
