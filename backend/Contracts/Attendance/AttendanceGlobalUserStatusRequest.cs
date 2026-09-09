namespace BDIP.Contracts.Attendance;

public sealed class AttendanceGlobalUserStatusRequest
{
    public Guid UserId { get; set; }
    public bool Enabled { get; set; }
}
