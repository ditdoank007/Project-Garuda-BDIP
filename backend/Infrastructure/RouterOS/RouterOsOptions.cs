namespace BDIP.Infrastructure.RouterOS;

public sealed class RouterOsOptions
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 8728;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool UseSsl { get; set; } = false;
}