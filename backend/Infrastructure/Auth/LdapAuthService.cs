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


        var isAdministrator = false;

        var adminSearch = new SearchRequest(
            _options.GroupsDn,
            "(cn=Administrators)",
            SearchScope.OneLevel,
            new[] { "member" });

        var adminResponse =
            (SearchResponse)adminConnection.SendRequest(adminSearch);

        if (adminResponse.Entries.Count == 1)
        {
            var members =
                adminResponse.Entries[0].Attributes["member"];

            if (members != null)
            {
                foreach (var item in members)
                {

                    if (item is byte[] bytes)
                    {
                    }

                    var memberDn =
                        item is byte[] memberBytes
                            ? System.Text.Encoding.UTF8.GetString(memberBytes)
                            : item?.ToString() ?? "";


                    if (string.Equals(
                        memberDn,
                        userDn,
                        StringComparison.OrdinalIgnoreCase))
                    {

                        isAdministrator = true;
                        break;
                    }
                }
            }
        }

        if (!isAdministrator)
        {
            throw new UnauthorizedAccessException(
                "Access denied. Only BDIP Administrators are allowed to sign in.");
        }

        var role = "Administrator";

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
