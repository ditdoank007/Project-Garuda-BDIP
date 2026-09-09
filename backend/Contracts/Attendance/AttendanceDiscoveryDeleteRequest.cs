namespace BDIP.Contracts.Attendance;

public sealed class AttendanceDiscoveryDeleteRequest
{
    public string MachineCode { get; set; } = string.Empty;

    public List<string> FingerIds { get; set; } = new();
}
