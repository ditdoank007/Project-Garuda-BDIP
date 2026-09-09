namespace BDIP.Contracts.Attendance;

public sealed class AttendanceReconciliationResponse
{
    public bool Success { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;

    public int MasterUserCount { get; set; }
    public int MachineUserCount { get; set; }

    public int MatchUserCount { get; set; }
    public int UserMissingCount { get; set; }
    public int TemplateMissingCount { get; set; }
    public int TemplateDifferentCount { get; set; }
    public int ExtraOnMachineCount { get; set; }

    public List<AttendanceReconciliationUser> Users { get; set; } = [];
}

public sealed class AttendanceReconciliationUser
{
    public string FingerId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool MasterEnabled { get; set; }
    public bool MachineRegistered { get; set; }
    public bool MachineEnabled { get; set; }

    public int MasterFingerprintCount { get; set; }
    public int MachineFingerprintCount { get; set; }

    public List<int> MissingFids { get; set; } = [];
    public List<int> DifferentFids { get; set; } = [];
}
