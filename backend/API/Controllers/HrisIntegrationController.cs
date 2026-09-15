using BDIP.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/integration/hris")]
public sealed class HrisIntegrationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public HrisIntegrationController(
        IUserService userService,
        IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpGet("pegawai")]
    public async Task<IActionResult> GetPegawai()
    {
        var configuredKey =
            _configuration["HrisIntegration:ApiKey"];

        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            return StatusCode(503, new
            {
                success = false,
                message = "HRIS integration API key belum dikonfigurasi."
            });
        }

        if (!Request.Headers.TryGetValue(
                "X-BDIP-Integration-Key",
                out var suppliedKey))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Integration key required."
            });
        }

        if (!string.Equals(
                suppliedKey.ToString(),
                configuredKey,
                StringComparison.Ordinal))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid integration key."
            });
        }

        var users = await _userService.GetUsersAsync();

        var data = users.Users
            .Where(user => !string.IsNullOrWhiteSpace(user.FingerId))
            .Select(user => new
            {
                fingerId = user.FingerId,
                nip = user.Nip,
                fullName = user.FullName,
                email = user.Email,
                unit = user.Unit,
                enabled = user.Enabled
            })
            .ToList();

        return Ok(new
        {
            success = true,
            message = "HRIS employee integration data loaded successfully.",
            data
        });
    }
}
