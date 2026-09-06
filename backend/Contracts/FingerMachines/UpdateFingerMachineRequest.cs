namespace BDIP.Contracts.FingerMachines;

public sealed class UpdateFingerMachineRequest
{
    public string Name { get; set; } = "";

    public string IpAddress { get; set; } = "";

    public int Port { get; set; } = 4370;

    public Guid? LocationId { get; set; }

    public string SerialNumber { get; set; } = "";

    public string DeviceName { get; set; } = "";

    public bool IsActive { get; set; } = true;
}
