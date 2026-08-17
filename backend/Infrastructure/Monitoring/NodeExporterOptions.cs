namespace BDIP.Infrastructure.Monitoring;

public sealed class NodeExporterOptions
{
    public ServerOptions Server { get; set; } = new();
    public ServerOptions Database { get; set; } = new();

    public sealed class ServerOptions
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
