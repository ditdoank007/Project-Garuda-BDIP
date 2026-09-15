namespace BDIP.Contracts.Audit;

public sealed class AuditLogResponse
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? Username { get; set; }

    public string? FullName { get; set; }

    public string? Role { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;

    public string? Target { get; set; }

    public string Result { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Details { get; set; }
}
