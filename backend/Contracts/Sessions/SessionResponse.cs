namespace BDIP.Contracts.Sessions;

public class SessionResponse
{
    public long RadAcctId { get; set; }

    public string AcctSessionId { get; set; } = "";

    public string Username { get; set; } = "";

    public string NasIpAddress { get; set; } = "";

    public string NasPortId { get; set; } = "";

    public string NasPortType { get; set; } = "";

    public DateTimeOffset? AcctStartTime { get; set; }

    public DateTimeOffset? AcctUpdateTime { get; set; }

    public DateTimeOffset? AcctStopTime { get; set; }

    public long? AcctSessionTime { get; set; }

    public long? AcctInputOctets { get; set; }

    public long? AcctOutputOctets { get; set; }

    public string AcctTerminateCause { get; set; } = "";

    public string CalledStationId { get; set; } = "";

    public string CallingStationId { get; set; } = "";

    public string ServiceType { get; set; } = "";

    public string FramedProtocol { get; set; } = "";

    public string FramedIpAddress { get; set; } = "";
}
