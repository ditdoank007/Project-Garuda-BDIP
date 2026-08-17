using BDIP.Infrastructure.Monitoring;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/monitoring")]
public sealed class MonitoringController : ControllerBase
{
    private readonly INodeExporterService _nodeExporterService;

    public MonitoringController(
        INodeExporterService nodeExporterService)
    {
        _nodeExporterService = nodeExporterService;
    }

    [HttpGet("servers")]
    public async Task<IActionResult> GetServers(
        CancellationToken cancellationToken)
    {
        var data =
            await _nodeExporterService
                .GetServerMetricsAsync(cancellationToken);

        return Ok(new
        {
            success = true,
            count = data.Count,
            data
        });
    }
}
