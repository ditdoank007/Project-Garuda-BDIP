namespace BDIP.Persistence.PostgreSQL;

public sealed class ApplicationDbOptions
{
    public string Host { get; set; } = "";

    public int Port { get; set; } = 5432;

    public string Database { get; set; } = "";

    public string Username { get; set; } = "";

    public string Password { get; set; } = "";
}