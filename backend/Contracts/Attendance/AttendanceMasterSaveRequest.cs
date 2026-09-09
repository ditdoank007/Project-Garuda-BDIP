namespace BDIP.Contracts.Attendance;

public sealed class AttendanceMasterSaveRequest
{
    public string? UserId { get; set; }
    public string FingerId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
