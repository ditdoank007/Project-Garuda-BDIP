namespace BDIP.Contracts.Audit;

public sealed class AuditLogListResponse
{
    public List<AuditLogResponse> Logs { get; set; } = new();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int Total { get; set; }

    public int TotalPages { get; set; }
}
