namespace BDIP.Contracts.Audit;

public sealed class AuditLogQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 50;

    public string? Username { get; set; }

    public string? Module { get; set; }

    public string? Action { get; set; }

    public string? Result { get; set; }

    public string? Search { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }
}
