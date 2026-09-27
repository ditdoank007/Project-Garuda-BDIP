using BDIP.API.Services.Analytics;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;

    public AnalyticsController(AnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] string? username,
        [FromQuery] string? access,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _analyticsService.GetAsync(
                    from,
                    to,
                    username,
                    access,
                    cancellationToken);

            return Ok(new
            {
                success = true,
                message = "Analytics loaded successfully",
                data = result
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
