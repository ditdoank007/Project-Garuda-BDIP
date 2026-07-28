using System.DirectoryServices.Protocols;
using System.Net;

using BDIP.Application.Auth;
using BDIP.Contracts.Auth;
using BDIP.Infrastructure.LDAP;
using Microsoft.Extensions.Options;

namespace BDIP.Infrastructure.Auth;

public class LdapAuthService : IAuthService
{
    private readonly ILdapConnectionFactory _ldap;
    private readonly LdapOptions _options;

    public LdapAuthService(
        ILdapConnectionFactory ldap,
        IOptions<LdapOptions> options)
    {
        _ldap = ldap;
        _options = options.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        await Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException(
                "Username and password are required.");
        }

        var username = request.Username.Trim();

        using var adminConnection = _ldap.Create();

        var searchRequest = new SearchRequest(
            _options.PeopleDn,
            $"(uid={EscapeFilterValue(username)})",
            SearchScope.OneLevel,
            new[]
            {
                "uid",
                "cn",
                "mail",
                "shadowExpire",
                "memberOf"
            });

        var searchResponse =
            (SearchResponse)adminConnection.SendRequest(searchRequest);

        if (searchResponse.Entries.Count != 1)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        var entry = searchResponse.Entries[0];

        var shadowExpire =
            entry.Attributes["shadowExpire"]?[0]?.ToString();

        if (!string.IsNullOrWhiteSpace(shadowExpire) &&
            shadowExpire != "-1")
        {
            throw new UnauthorizedAccessException(
                "This account is disabled.");
        }

        var userDn = entry.DistinguishedName;

        try
        {
            var identifier = new LdapDirectoryIdentifier(
                _options.Host,
                _options.Port);

            using var userConnection = new LdapConnection(identifier)
            {
                AuthType = AuthType.Basic,
                Credential = new NetworkCredential(
                    userDn,
                    request.Password)
            };

            userConnection.SessionOptions.ProtocolVersion = 3;
            userConnection.SessionOptions.SecureSocketLayer =
                _options.UseSsl;
            userConnection.Timeout = TimeSpan.FromSeconds(10);

            userConnection.Bind();
        }
        catch (LdapException)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        var memberOf = entry.Attributes["memberOf"];

        var role = "User";

        if (memberOf != null)
        {
            foreach (var item in memberOf)
            {
                var groupDn = item?.ToString() ?? "";

                if (groupDn.StartsWith(
                    "cn=Administrators,",
                    StringComparison.OrdinalIgnoreCase))
                {
                    role = "Administrator";
                    break;
                }
            }
        }

        return new LoginResponse
        {
            Username =
                entry.Attributes["uid"]?[0]?.ToString() ?? username,

            FullName =
                entry.Attributes["cn"]?[0]?.ToString() ?? username,

            Email =
                entry.Attributes["mail"]?[0]?.ToString() ?? "",

            Role = role
        };
    }

    private static string EscapeFilterValue(string value)
    {
        return value
            .Replace("\\", "\\5c")
            .Replace("*", "\\2a")
            .Replace("(", "\\28")
            .Replace(")", "\\29")
            .Replace("\0", "\\00");
    }
}
