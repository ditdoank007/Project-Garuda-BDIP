namespace BDIP.Domain.NAP;

public sealed class UserNap
{
    public string Uid { get; init; } = string.Empty;

    // Legacy (akan dihapus setelah migrasi selesai)
    public int DownloadKbps { get; set; }
    public int UploadKbps { get; set; }
    public int SessionTimeout { get; set; }
    public int IdleTimeout { get; set; }

    public Guid? PolicyId { get; set; }

    public string? PolicyCode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; set; }
}