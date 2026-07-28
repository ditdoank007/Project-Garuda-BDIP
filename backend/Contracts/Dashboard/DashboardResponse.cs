namespace BDIP.Contracts.Dashboard;

public class DashboardResponse
{
    public DashboardStats Stats { get; set; } = new();

    public List<DashboardActivity> Activities { get; set; } = new();
}

public class DashboardStats
{
    public int Users { get; set; }

    public int Groups { get; set; }

    public int Units { get; set; }

    public int Applications { get; set; }

    public string Ldap { get; set; } = "Healthy";
}

public class DashboardActivity
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Time { get; set; } = "";
}