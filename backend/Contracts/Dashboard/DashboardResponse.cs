using BDIP.Contracts.Synology;

namespace BDIP.Contracts.Dashboard;

public class DashboardResponse
{
    public DashboardStats Stats { get; set; } = new();

    public SynologyMonitoring Synology { get; set; } = new();

    public List<DashboardActivity> Activities { get; set; } = new();
}


public class SynologyMonitoring
{
    public bool Online { get; set; }

    public string Model { get; set; } = "";

    public string DsmVersion { get; set; } = "";

    public string VolumeName { get; set; } = "";

    public string VolumePath { get; set; } = "";

    public string FileSystem { get; set; } = "";

    public string RaidType { get; set; } = "";

    public string Status { get; set; } = "";

    public long TotalBytes { get; set; }

    public long UsedBytes { get; set; }

    public long FreeBytes { get; set; }

    public double UsedPercent { get; set; }

    public SynologyPerformance Performance { get; set; } = new();

    public SynologySystemResources SystemResources { get; set; } = new();

    public SynologyStorageHealth StorageHealth { get; set; } = new();

    public SynologyHardware Hardware { get; set; } = new();

    public SynologySystemHealth SystemHealth { get; set; } = new();
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

public class SynologyPerformance
{
    public double? ReadBytesPerSecond { get; set; }

    public double? WriteBytesPerSecond { get; set; }

    public double? ReadIops { get; set; }

    public double? WriteIops { get; set; }
}

public class SynologySystemResources
{
    public long? CpuPercent { get; set; }

    public long? MemoryPercent { get; set; }

    public long? TemperatureC { get; set; }

    public string? FanStatus { get; set; }
}

public class SynologyStorageHealth
{
    public string? RaidStatus { get; set; }

    public string? FilesystemStatus { get; set; }

    public string? DiskHealth { get; set; }

    public long? BadSectors { get; set; }
}

public class SynologyHardware
{
    public int BayCount { get; set; }

    public int DiskCount { get; set; }

    public int HealthyDisks { get; set; }

    public int WarningDisks { get; set; }

    public int FailedDisks { get; set; }

    public string PoolStatus { get; set; } = "";

    public string PoolRaidType { get; set; } = "";

    public int PoolDiskCount { get; set; }

    public long PoolTotalBytes { get; set; }

    public SynologySsdCache SsdCache { get; set; } = new();

    public List<SynologyDisk> Disks { get; set; } = new();
}

public class SynologyDisk
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public string Model { get; set; } = "";

    public string Vendor { get; set; } = "";

    public string Serial { get; set; } = "";

    public long CapacityBytes { get; set; }

    public string Status { get; set; } = "";

    public string SmartStatus { get; set; } = "";

    public double? Temperature { get; set; }

    public bool IsSsd { get; set; }

    public string RemainingLife { get; set; } = "";
}

public class SynologySsdCache
{
    public bool Enabled { get; set; }

    public string Status { get; set; } = "";

    public string RaidType { get; set; } = "";

    public int DiskCount { get; set; }

    public double? HitRate { get; set; }
}
