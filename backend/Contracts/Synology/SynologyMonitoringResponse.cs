namespace BDIP.Contracts.Synology;

public class SynologyMonitoringResponse
{
    public List<SynologyConnectionActivity> Connections { get; set; } = new();

    public bool Online { get; set; }

    public string Model { get; set; } = "";

    public string DsmVersion { get; set; } = "";

    public SynologyVolume Volume { get; set; } = new();

    public SynologySystemHealth SystemHealth { get; set; } = new();

    public SynologyHardware Hardware { get; set; } = new();
}

public class SynologyVolume
{
    public string Name { get; set; } = "";

    public string Path { get; set; } = "";

    public string FileSystem { get; set; } = "";

    public string RaidType { get; set; } = "";

    public string Status { get; set; } = "";

    public long TotalBytes { get; set; }

    public long FreeBytes { get; set; }

    public long UsedBytes => TotalBytes - FreeBytes;

    public double UsedPercent =>
        TotalBytes > 0
            ? Math.Round((double)UsedBytes / TotalBytes * 100, 2)
            : 0;
}


public class SynologyConnectionActivity
{
    public string User { get; set; } = "";

    public string SourceIp { get; set; } = "";

    public string Protocol { get; set; } = "";

    public string Type { get; set; } = "";

    public string Application { get; set; } = "";

    public string Time { get; set; } = "";

    public string FirstLoginTime { get; set; } = "";

    public bool CurrentConnected { get; set; }

    public string Location { get; set; } = "";

    public string UserAgent { get; set; } = "";

    public int Pid { get; set; }

    public string DeviceId { get; set; } = "";

    public bool CanBeKicked { get; set; }

    public bool IsAmfa { get; set; }

    public bool IsOtpTrusted { get; set; }
}



public class SynologySystemHealth
{
    public string Hostname { get; set; } = "";

    public string Uptime { get; set; } = "";

    public List<SynologyNetworkInterface> Interfaces { get; set; } = new();

    public bool Healthy { get; set; }
}

public class SynologyNetworkInterface
{
    public string Id { get; set; } = "";

    public string Ip { get; set; } = "";

    public string Type { get; set; } = "";
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
