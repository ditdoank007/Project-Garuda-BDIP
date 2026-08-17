namespace BDIP.Infrastructure.RouterOS;

public sealed class RouterOsActiveSession
{
    public string Id { get; set; } = "";

    public string User { get; set; } = "";

    public string Address { get; set; } = "";

    public string MacAddress { get; set; } = "";

    public string Server { get; set; } = "";

    public string Uptime { get; set; } = "";

    public long BytesIn { get; set; }

    public long BytesOut { get; set; }
}