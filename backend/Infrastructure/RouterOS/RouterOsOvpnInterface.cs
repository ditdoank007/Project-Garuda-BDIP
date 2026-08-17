namespace BDIP.Infrastructure.RouterOS;

public sealed class RouterOsOvpnInterface
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public string Type { get; set; } = "";

    public bool Running { get; set; }

    public long RxBytes { get; set; }

    public long TxBytes { get; set; }
}
