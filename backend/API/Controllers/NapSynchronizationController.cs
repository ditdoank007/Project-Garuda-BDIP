using BDIP.Application.NAP;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/nap")]
public sealed class NapSynchronizationController : ControllerBase
{
    private readonly INapSynchronizationService _service;

    public NapSynchronizationController(
        INapSynchronizationService service)
    {
        _service = service;
    }

    [HttpPost("synchronize")]
    public async Task<IActionResult> Synchronize()
    {
        var result = await _service.SynchronizeAsync();

        return Ok(result);
    }
}
