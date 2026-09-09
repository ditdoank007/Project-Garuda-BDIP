namespace BDIP.Contracts.Attendance;

public sealed class AttendanceDiscoveryImportResponse
{
    public bool Success { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public int ImportedUserCount { get; set; }
    public int ImportedTemplateCount { get; set; }
    public int SkippedAlreadyMatchedCount { get; set; }
}
