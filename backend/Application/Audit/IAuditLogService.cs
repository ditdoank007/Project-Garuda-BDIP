using BDIP.Contracts.Audit;

namespace BDIP.Application.Audit;

public interface IAuditLogService
{
    Task WriteAsync(
        string action,
        string module,
        string? username = null,
        string? fullName = null,
        string? role = null,
        string? target = null,
        string result = "SUCCESS",
        string? ipAddress = null,
        string? userAgent = null,
        string? details = null);

    Task<AuditLogListResponse> GetLogsAsync(
        AuditLogQuery query,
        CancellationToken cancellationToken = default);
}
