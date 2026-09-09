namespace BDIP.Contracts.Attendance;

public sealed class AttendanceSynchronizationResponse
{
    public bool Success { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset FinishedAt { get; set; }

    public int MachineCount { get; set; }

    public int SuccessMachineCount { get; set; }

    public int PendingMachineCount { get; set; }

    public int FailedMachineCount { get; set; }

    public List<AttendanceSynchronizationMachineResult> Machines { get; set; } = [];
}

public sealed class AttendanceSynchronizationMachineResult
{
    public string MachineCode { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Matched { get; set; }

    public int Created { get; set; }

    public int Updated { get; set; }

    public int Disabled { get; set; }

    public int Deleted { get; set; }

    public int TemplateWritten { get; set; }

    public string Message { get; set; } = string.Empty;
}
