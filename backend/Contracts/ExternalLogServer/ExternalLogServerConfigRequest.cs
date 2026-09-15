namespace BDIP.Contracts.ExternalLogServer;

public class ExternalLogServerConfigRequest
{
    public bool Enabled { get; set; }

    public string? ServerAddress { get; set; }

    public int Port { get; set; } = 514;

    public string Protocol { get; set; } = "UDP";

    public int Facility { get; set; } = 3;
}
