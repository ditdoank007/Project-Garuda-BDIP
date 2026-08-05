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