using BDIP.Application.Audit;
using BDIP.Contracts.Audit;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] AuditLogQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetLogsAsync(
            query,
            cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Audit logs loaded successfully",
            data = result
        });
    }
}
