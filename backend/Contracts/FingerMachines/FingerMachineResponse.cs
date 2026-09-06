namespace BDIP.Contracts.FingerMachines;

public sealed class FingerMachineResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public string IpAddress { get; set; } = "";

    public int Port { get; set; }

    public Guid? LocationId { get; set; }

    public string LocationName { get; set; } = "";

    public string SerialNumber { get; set; } = "";

    public string DeviceName { get; set; } = "";

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
