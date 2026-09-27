namespace BDIP.Contracts.Analytics;

public sealed class AnalyticsResponse
{
    public AnalyticsSummary Summary { get; set; } = new();

    public List<AnalyticsBreakdown> AccessBreakdown { get; set; } = [];

    public List<AnalyticsDailyPoint> Daily { get; set; } = [];

    public List<AnalyticsUser> TopUsers { get; set; } = [];

    public List<AnalyticsUser> TopDownloadUsers { get; set; } = [];

    public List<AnalyticsUser> TopUploadUsers { get; set; } = [];

    public List<AnalyticsUser> TopDurationUsers { get; set; } = [];

    public List<AnalyticsUser> TopOvpnUsers { get; set; } = [];
}

public sealed class AnalyticsSummary
{
    public int UniqueUsers { get; set; }

    public long TotalSessions { get; set; }

    public long TotalDurationSeconds { get; set; }

    public long TotalDownloadBytes { get; set; }

    public long TotalUploadBytes { get; set; }

    public long OvpnSessions { get; set; }

    public long OvpnDurationSeconds { get; set; }

    public long HotspotSessions { get; set; }

    public long HotspotDurationSeconds { get; set; }
}

public sealed class AnalyticsBreakdown
{
    public string Access { get; set; } = "";

    public long Sessions { get; set; }

    public long DurationSeconds { get; set; }

    public long DownloadBytes { get; set; }

    public long UploadBytes { get; set; }
}

public sealed class AnalyticsDailyPoint
{
    public DateTimeOffset Date { get; set; }

    public long Sessions { get; set; }

    public long DurationSeconds { get; set; }
}

public sealed class AnalyticsUser
{
    public string Username { get; set; } = "";

    public long Sessions { get; set; }

    public long DurationSeconds { get; set; }

    public long DownloadBytes { get; set; }

    public long UploadBytes { get; set; }

    public long OvpnSessions { get; set; }

    public long OvpnDurationSeconds { get; set; }

    public long HotspotSessions { get; set; }

    public long HotspotDurationSeconds { get; set; }
}
