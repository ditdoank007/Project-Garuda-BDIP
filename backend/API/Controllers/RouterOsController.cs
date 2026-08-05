using BDIP.Infrastructure.RouterOS;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/routeros")]
public sealed class RouterOsController
    : ControllerBase
{
    private readonly IRouterOsService _routerOsService;

    public RouterOsController(
        IRouterOsService routerOsService)
    {
        _routerOsService = routerOsService;
    }

    [HttpGet("test")]
    public async Task<IActionResult> TestConnection()
        {
            var connected =
                await _routerOsService
                    .TestConnectionAsync();

            return Ok(new
            {
                success = connected,
                connected
            });
        }
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
        {
            var data =
                await _routerOsService.GetHotspotActiveAsync();

                return Ok(new
                {
                    success = true,
                    data
                });
        }
        [HttpPost("disconnect/{id}")]
    public async Task<IActionResult> Disconnect(
        string id)
        {
                await _routerOsService
                    .DisconnectHotspotSessionAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Session disconnected."
                });
        }
        [HttpGet("ping")]
            public IActionResult Ping()
        {
                return Ok("routeros-ping");
        }
}