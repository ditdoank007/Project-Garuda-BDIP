using BDIP.Infrastructure.Synology;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/synology/snmp")]
public sealed class SynologySnmpController : ControllerBase
{
    private readonly SynologySnmpService _service;

    public SynologySnmpController(
        SynologySnmpService service)
    {
        _service = service;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test(
        CancellationToken cancellationToken)
    {
        var snapshot =
            await _service.GetSnapshotAsync(
                "192.168.33.200",
                cancellationToken);

        return Ok(snapshot);
    }
}
