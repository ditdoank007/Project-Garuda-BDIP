namespace BDIP.Contracts.Attendance;

public sealed class AttendanceDiscoveryImportRequest
{
    public string MachineCode { get; set; } = string.Empty;
    public List<string> FingerIds { get; set; } = new();
}
