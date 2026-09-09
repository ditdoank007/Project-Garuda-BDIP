namespace BDIP.Contracts.Attendance;

public sealed class AttendanceGlobalUserStatusResponse
{
    public bool Success { get; set; }
    public Guid UserId { get; set; }
    public bool RequestedEnabled { get; set; }
    public bool DatabaseUpdated { get; set; }
    public int MachineCount { get; set; }
    public int MachineUpdatedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}
