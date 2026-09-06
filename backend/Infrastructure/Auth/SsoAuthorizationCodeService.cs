using System.Collections.Concurrent;
using System.Security.Cryptography;
using BDIP.Application.Auth;
using BDIP.Contracts.Auth;

namespace BDIP.Infrastructure.Auth;

public class SsoAuthorizationCodeService : ISsoAuthorizationCodeService
{
    private sealed record Entry(
        LoginResponse User,
        string RedirectUri,
        DateTimeOffset ExpiresAt);

    private readonly ConcurrentDictionary<string, Entry> _codes = new();

    private static readonly TimeSpan Lifetime =
        TimeSpan.FromMinutes(2);

    public string Create(
        LoginResponse user,
        string redirectUri)
    {
        Cleanup();

        var code = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        _codes[code] = new Entry(
            user,
            redirectUri,
            DateTimeOffset.UtcNow.Add(Lifetime));

        return code;
    }

    public bool TryConsume(
        string code,
        string redirectUri,
        out LoginResponse user)
    {
        user = default!;

        if (!_codes.TryRemove(code, out var entry))
            return false;

        if (entry.ExpiresAt < DateTimeOffset.UtcNow)
            return false;

        if (!string.Equals(
                entry.RedirectUri,
                redirectUri,
                StringComparison.Ordinal))
            return false;

        user = entry.User;
        return true;
    }

    private void Cleanup()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var item in _codes)
        {
            if (item.Value.ExpiresAt < now)
                _codes.TryRemove(item.Key, out _);
        }
    }
}
