using BDIP.API.Services.Sessions;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly UnifiedSessionService _sessionService;

    public SessionsController(
        UnifiedSessionService sessionService)
    {
        _sessionService = sessionService;
    }
    [HttpGet("live-traffic")]
    public async Task<IActionResult> GetLiveTraffic(
        [FromQuery] string username,
        [FromQuery] string address,
        [FromQuery] string server)
    {
        try
        {
            var traffic =
                await _sessionService.GetLiveTrafficAsync(
                    username,
                    address,
                    server);

            return Ok(new
            {
                success = true,
                found = traffic.Found,
                rxBytes = traffic.RxBytes,
                txBytes = traffic.TxBytes
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "========== LIVE TRAFFIC ERROR ==========");

            Console.WriteLine(ex.ToString());

            Console.WriteLine(
                "=========================================");

            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions(
        CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine("Controller: before service");

            var sessions =
                await _sessionService.GetSessionsAsync(
                    cancellationToken);

            Console.WriteLine("Controller: service completed");

            return Ok(new
            {
                success = true,
                message = "Sessions loaded successfully",
                data = sessions
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("========== SESSION ERROR ==========");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("===================================");

            throw;
        }
    }
}