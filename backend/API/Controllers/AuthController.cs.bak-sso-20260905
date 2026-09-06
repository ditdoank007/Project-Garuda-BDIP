using BDIP.Application.Auth;
using BDIP.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string SessionCookieName = "bdip_session";

    private readonly IAuthService _authService;
    private readonly IBdipSessionService _sessionService;

    public AuthController(
        IAuthService authService,
        IBdipSessionService sessionService)
    {
        _authService = authService;
        _sessionService = sessionService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        try
        {
            var user = await _authService.LoginAsync(request);

            var token = _sessionService.Create(user);

            Response.Cookies.Append(
                SessionCookieName,
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });

            return Ok(new
            {
                success = true,
                message = "Login successful.",
                data = user
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        if (!_sessionService.TryRead(
            Request.Cookies[SessionCookieName],
            out var user))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Session is invalid or expired."
            });
        }

        return Ok(new
        {
            success = true,
            data = user
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(
            SessionCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

        return Ok(new
        {
            success = true,
            message = "Logged out successfully."
        });
    }
}
