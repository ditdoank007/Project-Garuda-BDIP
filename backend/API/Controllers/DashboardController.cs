using BDIP.Application.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();

        return Ok(new
        {
            success = true,
            message = "Dashboard loaded successfully",
            data = dashboard
        });
    }
}