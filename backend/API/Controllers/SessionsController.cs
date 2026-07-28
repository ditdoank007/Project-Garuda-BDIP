using BDIP.Application.Sessions;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(
        ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions(
        CancellationToken cancellationToken)
    {
        var sessions =
            await _sessionService.GetSessionsAsync(
                cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Sessions loaded successfully",
            data = sessions
        });
    }
}
