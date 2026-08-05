namespace BDIP.Domain.NAP;

public sealed class Policy
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Session
    public int SessionTimeout { get; set; }
    public int IdleTimeout { get; set; }
    public int SimultaneousUse { get; set; } = 1;

    // Bandwidth
    public int DownloadRate { get; set; }
    public int UploadRate { get; set; }
    public int? BurstDownload { get; set; }
    public int? BurstUpload { get; set; }
    public int? Priority { get; set; }

    // Quota
    public long? DailyQuota { get; set; }
    public long? MonthlyQuota { get; set; }
    public long? TotalQuota { get; set; }

    // Network
    public string? AddressList { get; set; }
    public int? VlanId { get; set; }
    public string? IpPool { get; set; }

    // Access
    public bool Enabled { get; set; } = true;
    public DateTime? ExpirationDate { get; set; }
    public string? LoginSchedule { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}