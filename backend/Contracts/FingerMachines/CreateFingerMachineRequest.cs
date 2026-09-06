namespace BDIP.Contracts.FingerMachines;

public sealed class CreateFingerMachineRequest
{
    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public string IpAddress { get; set; } = "";

    public int Port { get; set; } = 4370;

    public Guid? LocationId { get; set; }
}
