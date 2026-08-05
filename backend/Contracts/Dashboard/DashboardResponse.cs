namespace BDIP.Contracts.Dashboard;

public class DashboardResponse
{
    public DashboardStats Stats { get; set; } = new();

    public List<DashboardActivity> Activities { get; set; } = new();
}

public class DashboardStats
{
    // Executive Dashboard KPI
    public int TotalUsers { get; set; }

    public int ActiveSessions { get; set; }

    public int HotspotSessions { get; set; }

    public int VpnSessions { get; set; }

    public int TotalPolicies { get; set; }

    public int NasOnline { get; set; }

    public int Applications { get; set; }

    // Supporting Information
    public int Groups { get; set; }

    public int Units { get; set; }

    public string Ldap { get; set; } = "Healthy";
}

public class DashboardActivity
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Time { get; set; } = "";
}