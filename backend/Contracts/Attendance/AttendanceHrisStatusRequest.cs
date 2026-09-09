namespace BDIP.Contracts.Attendance;

public sealed class AttendanceHrisStatusRequest
{
    public string FingerId { get; set; } = string.Empty;
    public string? IsKeluar { get; set; }
}
