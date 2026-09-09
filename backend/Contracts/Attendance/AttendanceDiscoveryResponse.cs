namespace BDIP.Contracts.Attendance;

public sealed class AttendanceDiscoveryResponse
{
    public bool Success { get; set; }

    public string MachineCode { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    public int DeviceUserCount { get; set; }

    public int DeviceTemplateCount { get; set; }

    public int MatchedUserCount { get; set; }

    public int NewUserCount { get; set; }

    public List<AttendanceDiscoveryUser> Users { get; set; } = new();
}

public sealed class AttendanceDiscoveryUser
{
    public int DeviceUid { get; set; }

    public string FingerId { get; set; } = string.Empty;

    // Nama yang terbaca langsung dari mesin.
    // BUKAN master nama BDIP.
    public string DeviceName { get; set; } = string.Empty;

    // Nama master dari BDIP Users.
    public string BdipFullName { get; set; } = string.Empty;

    public bool IsMatched { get; set; }

    public int TemplateCount { get; set; }
}
