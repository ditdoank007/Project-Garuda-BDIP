namespace BDIP.Contracts.Attendance;

public sealed class AttendanceMachineUserEnabledRequest
{
    public string MachineCode { get; set; } = string.Empty;

    public int DeviceUid { get; set; }

    public bool Enabled { get; set; }
}
