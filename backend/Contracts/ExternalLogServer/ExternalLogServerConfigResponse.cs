namespace BDIP.Contracts.ExternalLogServer;

public class ExternalLogServerConfigResponse
{
    public bool Enabled { get; set; }

    public string? ServerAddress { get; set; }

    public int Port { get; set; }

    public string Protocol { get; set; } = "UDP";

    public int Facility { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
