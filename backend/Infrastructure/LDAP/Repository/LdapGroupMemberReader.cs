using System.DirectoryServices.Protocols;

using BDIP.Application.Groups;
using BDIP.Contracts.Groups;

namespace BDIP.Infrastructure.LDAP.Repository;

public class LdapGroupMemberReader : IGroupMemberReader
{
    private const string DummyMemberDn =
        "cn=dummy,dc=basarnas,dc=go,dc=id";

    private readonly ILdapConnectionFactory _ldap;
    private readonly LdapOptions _options;

    public LdapGroupMemberReader(
        ILdapConnectionFactory ldap,
        Microsoft.Extensions.Options.IOptions<LdapOptions> options)
    {
        _ldap = ldap;
        _options = options.Value;
    }

    public async Task<GroupMembersResponse?> GetMembersAsync(
        string groupName)
    {
        await Task.CompletedTask;

        using var connection = _ldap.Create();

        var groupRequest = new SearchRequest(
            _options.GroupsDn,
            $"(cn={EscapeFilterValue(groupName)})",
            SearchScope.OneLevel,
            new[] { "cn", "member" });

        var groupResponse =
            (SearchResponse)connection.SendRequest(groupRequest);

        if (groupResponse.Entries.Count == 0)
            return null;

        var group = groupResponse.Entries[0];

        var result = new GroupMembersResponse
        {
            GroupName =
                group.Attributes["cn"]?[0]?.ToString()
                ?? groupName
        };

        var members = group.Attributes["member"];

        if (members == null)
            return result;

        for (int i = 0; i < members.Count; i++)
        {
            var memberDn = members[i]?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(memberDn))
                continue;

            if (string.Equals(
                memberDn,
                DummyMemberDn,
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                var userRequest = new SearchRequest(
                    memberDn,
                    "(objectClass=*)",
                    SearchScope.Base,
                    new[]
                    {
                        "uid",
                        "cn",
                        "mail",
                        "ou"
                    });

                var userResponse =
                    (SearchResponse)connection.SendRequest(
                        userRequest);

                if (userResponse.Entries.Count == 0)
                    continue;

                var user = userResponse.Entries[0];

                result.Members.Add(
                    new GroupMemberResponse
                    {
                        Username =
                            user.Attributes["uid"]?[0]?.ToString()
                            ?? "",

                        FullName =
                            user.Attributes["cn"]?[0]?.ToString()
                            ?? "",

                        Email =
                            user.Attributes["mail"]?[0]?.ToString()
                            ?? "",

                        Unit =
                            user.Attributes["ou"]?[0]?.ToString()
                            ?? "",

                        DistinguishedName =
                            user.DistinguishedName
                    });
            }
            catch (DirectoryOperationException)
            {
                // DN anggota sudah tidak ada; lewati agar
                // anggota lain tetap dapat ditampilkan.
            }
        }

        return result;
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
