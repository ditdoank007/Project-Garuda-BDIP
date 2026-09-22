using BDIP.Application.Audit;
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
    private readonly IAuditLogService _auditLogService;

    public AuthController(
        IAuthService authService,
        IBdipSessionService sessionService,
        ISsoAuthorizationCodeService ssoService,
        IAuditLogService auditLogService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _ssoService = ssoService;
        _auditLogService = auditLogService;
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

            try
            {
                await _auditLogService.WriteAsync(
                    action: "LOGIN",
                    module: "AUTH",
                    username: user.Username,
                    fullName: user.FullName,
                    role: user.Role,
                    target: null,
                    result: "SUCCESS",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers.UserAgent.FirstOrDefault(),
                    details: "User login successful.");
            }
            catch (Exception auditEx)
            {
                Console.WriteLine(
                    $"[AUDIT] LOGIN write failed: {auditEx.Message}");
            }

            return Ok(new
            {
                success = true,
                message = "Login successful.",
                data = user
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            try
            {
                await _auditLogService.WriteAsync(
                    action: "LOGIN_FAILED",
                    module: "AUTH",
                    username: request.Username,
                    fullName: request.Username,
                    role: "User",
                    target: null,
                    result: "FAILED",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers.UserAgent.FirstOrDefault(),
                    details: "Authentication failed.");
            }
            catch (Exception auditEx)
            {
                Console.WriteLine(
                    $"[AUDIT] LOGIN_FAILED write failed: {auditEx.Message}");
            }

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
    public async Task<IActionResult> Logout()
    {
        _sessionService.TryRead(
            Request.Cookies[SessionCookieName],
            out var currentUser);

        try
        {
            if (!string.IsNullOrWhiteSpace(currentUser.Username))
            {
                await _auditLogService.WriteAsync(
                    action: "LOGOUT",
                    module: "AUTH",
                    username: currentUser.Username,
                    fullName: currentUser.FullName,
                    role: currentUser.Role,
                    target: null,
                    result: "SUCCESS",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers.UserAgent.FirstOrDefault(),
                    details: "User logout successful.");
            }
        }
        catch (Exception auditEx)
        {
            Console.WriteLine(
                $"[AUDIT] LOGOUT write failed: {auditEx.Message}");
        }

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
