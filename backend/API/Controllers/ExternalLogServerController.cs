using BDIP.Application.ExternalLogServer;
using BDIP.Contracts.ExternalLogServer;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/external-log-server")]
public class ExternalLogServerController : ControllerBase
{
    private readonly IExternalLogServerConfigService _service;

    public ExternalLogServerController(
        IExternalLogServerConfigService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var config = await _service.GetAsync(cancellationToken);

        return Ok(new
        {
            success = true,
            data = config
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] ExternalLogServerConfigRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var config = await _service.UpdateAsync(
                request,
                cancellationToken);

            return Ok(new
            {
                success = true,
                message = "External log server configuration updated successfully.",
                data = config
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}
