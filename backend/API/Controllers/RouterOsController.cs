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
        [HttpGet("ppp-active")]
    public async Task<IActionResult> GetPppActive()
    {
        var data =
            await _routerOsService.GetPppActiveAsync();

        return Ok(new
        {
            success = true,
            count = data.Count,
            data
        });
    }

    [HttpGet("ppp-raw")]
    public async Task<IActionResult> GetPppRaw()
    {
        var data =
            await _routerOsService.GetPppRawAsync();

        return Ok(new
        {
            success = true,
            data
        });
    }

    [HttpGet("ovpn-traffic")]
    public async Task<IActionResult> GetOvpnTraffic()
    {
        var data =
            await _routerOsService.GetOvpnTrafficAsync();

        return Ok(new
        {
            success = true,
            count = data.Count,
            data
        });
    }

    [HttpGet("interfaces")]
    public async Task<IActionResult> GetInterfaces()
    {
        var data =
            await _routerOsService.GetOvpnInterfacesAsync();

        return Ok(new
        {
            success = true,
            count = data.Count,
            data
        });
    }

    [HttpGet("interface-raw")]
    public async Task<IActionResult> GetInterfaceRaw()
    {
        var data =
            await _routerOsService.GetOvpnInterfacesRawAsync();

        return Ok(new
        {
            success = true,
            data
        });
    }

    [HttpGet("ping")]
            public IActionResult Ping()
        {
                return Ok("routeros-ping");
        }
}