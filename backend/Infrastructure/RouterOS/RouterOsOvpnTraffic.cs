namespace BDIP.Infrastructure.RouterOS;

public sealed class RouterOsOvpnTraffic
{
    public string User { get; set; } = "";

    public string Address { get; set; } = "";

    public string InterfaceName { get; set; } = "";

    public bool Running { get; set; }

    public long RxBytes { get; set; }

    public long TxBytes { get; set; }
}
