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

    // ---------- RouterOS Live ----------

    public string RouterOsId { get; set; } = "";

    public string RouterAddress { get; set; } = "";

    public string MacAddress { get; set; } = "";

    public string RouterServer { get; set; } = "";

    public bool IsRouterActive { get; set; }

    public string RouterOsInterface { get; set; } = "";

    public long RouterOsRxBytes { get; set; }

    public long RouterOsTxBytes { get; set; }

    // ---------- NAP Enrichment ----------

    public string PolicyCode { get; set; } = "";

    public string PolicyName { get; set; } = "";

    public int? DownloadRate { get; set; }

    public int? UploadRate { get; set; }

    public int? SessionTimeout { get; set; }

    public int? IdleTimeout { get; set; }

    public int? SimultaneousUse { get; set; }
}
