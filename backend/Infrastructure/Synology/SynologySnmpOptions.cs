namespace BDIP.Infrastructure.Synology;

public sealed class SynologySnmpOptions
{
    public string Username { get; set; } = string.Empty;

    public string AuthPassword { get; set; } = string.Empty;

    public string PrivPassword { get; set; } = string.Empty;

    public string AuthProtocol { get; set; } = "SHA";

    public string PrivProtocol { get; set; } = "AES";
}
