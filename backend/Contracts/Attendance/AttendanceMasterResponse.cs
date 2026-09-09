namespace BDIP.Contracts.Attendance;

public sealed class AttendanceMasterResponse
{
    public List<AttendanceMasterUser> Users { get; set; } = new();
}

public sealed class AttendanceMasterUser
{
    public Guid UserId { get; set; }
    public bool IsLinked { get; set; }

    public string Nip { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string FingerId { get; set; } = string.Empty;

    public bool UserEnabled { get; set; }

    public int FingerprintCount { get; set; }

    public List<AttendanceMasterMachine> Machines { get; set; } = new();
}

public sealed class AttendanceMasterMachine
{
    public string MachineCode { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    public bool Registered { get; set; }

    public bool Enabled { get; set; }

    public int DeviceUid { get; set; }

    public DateTime? LastSeenAt { get; set; }
}
