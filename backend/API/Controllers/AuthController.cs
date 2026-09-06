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
    private readonly ISsoAuthorizationCodeService _ssoService;

    public AuthController(
        IAuthService authService,
        IBdipSessionService sessionService,
        ISsoAuthorizationCodeService ssoService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _ssoService = ssoService;
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

    [HttpGet("sso/start")]
    public IActionResult SsoStart(
        [FromQuery] string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            return BadRequest(new
            {
                success = false,
                message = "redirectUri is required."
            });
        }

        if (!_sessionService.TryRead(
                Request.Cookies[SessionCookieName],
                out var user))
        {
            return Unauthorized(new
            {
                success = false,
                message = "BDIP login required."
            });
        }

        var code = _ssoService.Create(user, redirectUri);

        return Redirect($"{redirectUri}?code={Uri.EscapeDataString(code)}");
    }

    [HttpPost("sso/exchange")]
    public IActionResult SsoExchange(
        [FromBody] SsoExchangeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) ||
            string.IsNullOrWhiteSpace(request.RedirectUri))
        {
            return BadRequest(new
            {
                success = false,
                message = "code and redirectUri are required."
            });
        }

        if (!_ssoService.TryConsume(
                request.Code,
                request.RedirectUri,
                out var user))
        {
            return Unauthorized(new
            {
                success = false,
                message = "Authorization code is invalid or expired."
            });
        }

        return Ok(new
        {
            success = true,
            data = user
        });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(
        [FromBody] LoginRequest request)
    {
        try
        {
            var user = await _authService.VerifyCredentialsAsync(request);

            return Ok(new
            {
                success = true,
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
