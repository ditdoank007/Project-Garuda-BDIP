namespace BDIP.Contracts.Attendance;

public sealed class AttendanceMachineUserEnabledResponse
{
    public bool Success { get; set; }

    public string MachineCode { get; set; } = string.Empty;

    public int DeviceUid { get; set; }

    public bool RequestedEnabled { get; set; }

    public bool DeviceEnabled { get; set; }

    public bool DatabaseUpdated { get; set; }

    public string Message { get; set; } = string.Empty;
}
