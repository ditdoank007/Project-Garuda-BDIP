using BDIP.Application.Auth;

namespace BDIP.API.Middleware;

public sealed class GlobalAuthorizationMiddleware
{
    private const string SessionCookieName = "bdip_session";
    private const string InternalSecretHeader = "X-BDIP-Internal-Secret";
    private const string InternalSecretConfig = "BDIP_INTERNAL_API_SECRET";

    private readonly RequestDelegate _next;

    public GlobalAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IBdipSessionService sessionService,
        IConfiguration configuration)
    {
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;

        Console.WriteLine(
            $"[AUTH-GUARD] {method} {path} | Cookie={(context.Request.Cookies.ContainsKey(SessionCookieName) ? "YES" : "NO")}");

        // Endpoint authentication memang harus dapat diakses
        // tanpa session.
        if (path.StartsWith(
                "/api/auth",
                StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Hanya API yang dikenakan global authorization.
        if (!path.StartsWith(
                "/api/",
                StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Frontend SSR boleh melakukan READ melalui jaringan internal.
        // Secret internal hanya berlaku untuk GET/HEAD/OPTIONS.
        if (HttpMethods.IsGet(method) ||
            HttpMethods.IsHead(method) ||
            HttpMethods.IsOptions(method))
        {
            var configuredSecret =
                configuration[InternalSecretConfig];

            var receivedSecret =
                context.Request.Headers[InternalSecretHeader]
                    .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(configuredSecret) &&
                string.Equals(
                    receivedSecret,
                    configuredSecret,
                    StringComparison.Ordinal))
            {
                await _next(context);
                return;
            }
        }

        // Browser/API request harus mempunyai session valid.
        var token =
            context.Request.Cookies[SessionCookieName];

        if (!sessionService.TryRead(token, out var user))
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Authentication required."
            });

            return;
        }

        // Administrator mempunyai full access.
        if (string.Equals(
                user.Role,
                "Administrator",
                StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Log Activity dan konfigurasi External Log Server
        // hanya dapat diakses oleh Administrator.
        if (path.StartsWith(
                "/api/audit",
                StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(
                "/api/external-log-server",
                StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "You do not have permission to access this resource."
            });

            return;
        }

        // User biasa boleh melakukan READ.
        if (HttpMethods.IsGet(method) ||
            HttpMethods.IsHead(method) ||
            HttpMethods.IsOptions(method))
        {
            await _next(context);
            return;
        }

        // User biasa hanya boleh reset password dirinya sendiri.
        if (HttpMethods.IsPost(method) &&
            path.StartsWith(
                "/api/users/",
                StringComparison.OrdinalIgnoreCase) &&
            path.EndsWith(
                "/reset-password",
                StringComparison.OrdinalIgnoreCase))
        {
            const string prefix = "/api/users/";
            const string suffix = "/reset-password";

            var username = path.Substring(
                prefix.Length,
                path.Length - prefix.Length - suffix.Length);

            if (string.Equals(
                    username,
                    user.Username,
                    StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }
        }

        // Semua operasi write user biasa ditolak.
        context.Response.StatusCode =
            StatusCodes.Status403Forbidden;

        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = "You do not have permission to modify data."
        });
    }
}
