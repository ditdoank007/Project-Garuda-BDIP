using System.DirectoryServices.Protocols;

using BDIP.Application.Groups;

namespace BDIP.Infrastructure.LDAP.Repository;

public class LdapUserDnResolver : IUserDnResolver
{
    private readonly ILdapConnectionFactory _ldap;
    private readonly LdapOptions _options;

    public LdapUserDnResolver(
        ILdapConnectionFactory ldap,
        Microsoft.Extensions.Options.IOptions<LdapOptions> options)
    {
        _ldap = ldap;
        _options = options.Value;
    }

    public async Task<string?> GetUserDnAsync(string username)
    {
        await Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(username))
            return null;

        using var connection = _ldap.Create();

        var request = new SearchRequest(
            _options.PeopleDn,
            $"(uid={EscapeFilterValue(username.Trim())})",
            SearchScope.Subtree,
            new[] { "uid" });

        var response =
            (SearchResponse)connection.SendRequest(request);

        return response.Entries.Count == 0
            ? null
            : response.Entries[0].DistinguishedName;
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
