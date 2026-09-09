namespace BDIP.Contracts.Attendance;

public sealed class AttendanceHrisStatusResponse
{
    public bool Success { get; set; }
    public string FingerId { get; set; } = string.Empty;
    public string? HrisStatus { get; set; }
    public bool UserFound { get; set; }
    public bool StatusChanged { get; set; }
    public int MachineCount { get; set; }
    public int MachineUpdatedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}
