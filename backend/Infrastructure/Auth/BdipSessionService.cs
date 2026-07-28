using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using BDIP.Application.Auth;
using BDIP.Contracts.Auth;

namespace BDIP.Infrastructure.Auth;

public class BdipSessionService : IBdipSessionService
{
    private readonly byte[] _secret;

    public BdipSessionService(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
        {
            throw new InvalidOperationException(
                "BdipSession secret must contain at least 32 characters.");
        }

        _secret = Encoding.UTF8.GetBytes(secret);
    }

    public string Create(LoginResponse user)
    {
        var payload = new SessionPayload
        {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ExpiresAtUnix = DateTimeOffset.UtcNow
                .AddHours(8)
                .ToUnixTimeSeconds()
        };

        var payloadPart = Base64UrlEncode(
            Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(payload)));

        var signaturePart = Base64UrlEncode(Sign(payloadPart));

        return $"{payloadPart}.{signaturePart}";
    }

    public bool TryRead(string? token, out LoginResponse user)
    {
        user = new LoginResponse();

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var parts = token.Split('.', 2);

        if (parts.Length != 2)
        {
            return false;
        }

        try
        {
            var receivedSignature = Base64UrlDecode(parts[1]);
            var expectedSignature = Sign(parts[0]);

            if (receivedSignature.Length != expectedSignature.Length ||
                !CryptographicOperations.FixedTimeEquals(
                    receivedSignature,
                    expectedSignature))
            {
                return false;
            }

            var payload = JsonSerializer.Deserialize<SessionPayload>(
                Encoding.UTF8.GetString(
                    Base64UrlDecode(parts[0])));

            if (payload == null ||
                string.IsNullOrWhiteSpace(payload.Username) ||
                payload.ExpiresAtUnix <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return false;
            }

            user = new LoginResponse
            {
                Username = payload.Username,
                FullName = payload.FullName ?? payload.Username,
                Email = payload.Email ?? "",
                Role = payload.Role ?? "User"
            };

            return true;
        }
        catch
        {
            return false;
        }
    }

    private byte[] Sign(string payloadPart)
    {
        using var hmac = new HMACSHA256(_secret);

        return hmac.ComputeHash(
            Encoding.UTF8.GetBytes(payloadPart));
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value
            .Replace('-', '+')
            .Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
            case 1:
                throw new FormatException("Invalid Base64Url value.");
        }

        return Convert.FromBase64String(base64);
    }

    private sealed class SessionPayload
    {
        public string Username { get; set; } = "";
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public long ExpiresAtUnix { get; set; }
    }
}
