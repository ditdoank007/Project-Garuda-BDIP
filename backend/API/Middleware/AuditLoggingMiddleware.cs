using BDIP.Application.Audit;
using BDIP.Application.Auth;

namespace BDIP.API.Middleware;

public sealed class AuditLoggingMiddleware
{
    private const string SessionCookieName = "bdip_session";

    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IBdipSessionService sessionService,
        IAuditLogService auditLogService)
    {
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;

        if (!path.StartsWith(
                "/api/",
                StringComparison.OrdinalIgnoreCase) ||
            HttpMethods.IsGet(method) ||
            HttpMethods.IsHead(method) ||
            HttpMethods.IsOptions(method) ||
            path.StartsWith(
                "/api/auth",
                StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(
                "/api/audit",
                StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var token =
            context.Request.Cookies[SessionCookieName];

        if (!sessionService.TryRead(token, out var user))
        {
            await _next(context);
            return;
        }

        await _next(context);

        if (context.Response.StatusCode < 200 ||
            context.Response.StatusCode >= 300)
        {
            return;
        }

        var segments = path
            .Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

        var module = segments.Length > 1
            ? segments[1].ToUpperInvariant()
            : "API";

        var target = path;

        var upperPath = path.ToUpperInvariant();

        var action =
            upperPath.EndsWith("/RESET-PASSWORD", StringComparison.Ordinal)
                ? "RESET_PASSWORD"
                : upperPath.EndsWith("/EMAIL", StringComparison.Ordinal)
                    ? "UPDATE_EMAIL"
                    : upperPath.EndsWith("/POLICY", StringComparison.Ordinal)
                        ? "UPDATE_POLICY"
                        : method.ToUpperInvariant() switch
                        {
                            "POST" => "CREATE",
                            "PUT" => "UPDATE",
                            "PATCH" => "UPDATE",
                            "DELETE" => "DELETE",
                            _ => method.ToUpperInvariant()
                        };

        var ipAddress =
            context.Connection.RemoteIpAddress?.ToString();

        var userAgent =
            context.Request.Headers.UserAgent
                .FirstOrDefault();

        try
        {
            await auditLogService.WriteAsync(
                action: action,
                module: module,
                username: user.Username,
                fullName: user.FullName,
                role: user.Role,
                target: target,
                result: "SUCCESS",
                ipAddress: ipAddress,
                userAgent: userAgent,
                details: $"{method} {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[AUDIT] Failed to write audit log: {ex.Message}");
        }
    }
}
