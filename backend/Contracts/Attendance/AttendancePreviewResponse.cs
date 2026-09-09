namespace BDIP.Contracts.Attendance;

public sealed class AttendancePreviewResponse
{
    public bool Success { get; set; }

    public string MachineCode { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    public int DeviceUserCount { get; set; }

    public int DeviceTemplateCount { get; set; }

    public int MatchedUserCount { get; set; }

    public int UnmatchedUserCount { get; set; }

    public List<AttendancePreviewUser> Users { get; set; } = new();
}

public sealed class AttendancePreviewUser
{
    public int DeviceUid { get; set; }

    public string DeviceUserId { get; set; } = string.Empty;

    public string DeviceName { get; set; } = string.Empty;

    public string FingerId { get; set; } = string.Empty;

    public bool Matched { get; set; }

    public int TemplateCount { get; set; }
}
