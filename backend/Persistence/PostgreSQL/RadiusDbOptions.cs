namespace BDIP.Persistence.PostgreSQL;

public sealed class RadiusDbOptions
{
    public string Host { get; set; } = "";

    public int Port { get; set; }

    public string Database { get; set; } = "";

    public string Username { get; set; } = "";

    public string Password { get; set; } = "";
}