using System.DirectoryServices.Protocols;
using BDIP.Application.NAP;
using BDIP.Infrastructure.LDAP;
using BDIP.Domain.NAP;
using Microsoft.Extensions.Options;

namespace BDIP.Infrastructure.NAP;

public sealed class NapLdapGroupSyncService : INapLdapGroupSyncService
{
    private readonly ILdapConnectionFactory _connectionFactory;
    private readonly LdapOptions _ldapOptions;
    private readonly IUserNapService _userNapService;
    private readonly IPolicyService _policyService;

    public NapLdapGroupSyncService(
        ILdapConnectionFactory connectionFactory,
        IOptions<LdapOptions> ldapOptions,
        IUserNapService userNapService,
        IPolicyService policyService)
    {
        _connectionFactory = connectionFactory;
        _ldapOptions = ldapOptions.Value;
        _userNapService = userNapService;
        _policyService = policyService;
    }

    public async Task SyncAllAsync()
    {
        var policies =
            (await _policyService.GetAllAsync())
            .Where(x => x.IsActive && x.Enabled)
            .ToList();

        var users =
            await _userNapService.GetAllAsync();

        using var connection =
            _connectionFactory.Create();

        var usedGids =
            LoadUsedGids(connection);

        foreach (var policy in policies)
        {
            var members = users
                .Where(x =>
                    x.PolicyId == policy.Id ||
                    (!string.IsNullOrWhiteSpace(policy.Code) &&
                     string.Equals(
                         x.PolicyCode,
                         policy.Code,
                         StringComparison.OrdinalIgnoreCase)))
                .Select(x => x.Uid)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            await SyncGroupAsync(
                connection,
                policy,
                members,
                usedGids);
        }
    }

    private async Task SyncGroupAsync(
        LdapConnection connection,
        Policy policy,
        IReadOnlyList<string> members,
        HashSet<int> usedGids)
    {
        var groupName =
            NormalizeGroupName(policy.Name, policy.Code);

        var dn =
            $"cn={EscapeDn(groupName)},{_ldapOptions.GroupsDn}";

        var existing =
            FindGroup(connection, groupName);

        if (existing is null)
        {
            var gid =
                GenerateGidNumber(usedGids);

            usedGids.Add(gid);

            var add =
                new AddRequest(dn);

            add.Attributes.Add(
                new DirectoryAttribute(
                    "objectClass",
                    "top",
                    "posixGroup"));

            add.Attributes.Add(
                new DirectoryAttribute(
                    "cn",
                    groupName));

            add.Attributes.Add(
                new DirectoryAttribute(
                    "gidNumber",
                    gid.ToString()));

            if (members.Count > 0)
            {
                add.Attributes.Add(
                    new DirectoryAttribute(
                        "memberUid",
                        members.ToArray()));
            }

            await Task.Run(
                () => connection.SendRequest(add));

            return;
        }

        var objectClasses =
            existing.Attributes["objectClass"]?
                .GetValues(typeof(string))
                .Cast<string>()
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        var isPosixGroup =
            objectClasses.Contains("posixGroup");

        if (!isPosixGroup)
        {
            throw new InvalidOperationException(
                $"LDAP group '{groupName}' already exists but is not posixGroup. " +
                "Automatic structural conversion is intentionally blocked.");
        }

        var currentMembers =
            existing.Attributes["memberUid"]?
                .GetValues(typeof(string))
                .Cast<string>()
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        var desiredMembers =
            members.ToHashSet(
                StringComparer.OrdinalIgnoreCase);

        var modifications =
            new List<DirectoryAttributeModification>();

        var toAdd =
            desiredMembers
                .Except(currentMembers,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        var toRemove =
            currentMembers
                .Except(desiredMembers,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (toAdd.Length > 0)
        {
            var add =
                new DirectoryAttributeModification
                {
                    Name = "memberUid",
                    Operation =
                        DirectoryAttributeOperation.Add
                };

            foreach (var member in toAdd)
                add.Add(member);

            modifications.Add(add);
        }

        if (toRemove.Length > 0)
        {
            var remove =
                new DirectoryAttributeModification
                {
                    Name = "memberUid",
                    Operation =
                        DirectoryAttributeOperation.Delete
                };

            foreach (var member in toRemove)
                remove.Add(member);

            modifications.Add(remove);
        }

        if (modifications.Count == 0)
            return;

        var modify =
            new ModifyRequest(
                existing.DistinguishedName);

        foreach (var modification in modifications)
            modify.Modifications.Add(modification);

        await Task.Run(
            () => connection.SendRequest(modify));
    }

    private static SearchResultEntry? FindGroup(
        LdapConnection connection,
        string groupName)
    {
        var request =
            new SearchRequest(
                "ou=Groups,dc=basarnas,dc=go,dc=id",
                $"(cn={EscapeFilter(groupName)})",
                SearchScope.OneLevel,
                "cn",
                "gidNumber",
                "memberUid",
                "objectClass");

        var response =
            (SearchResponse)connection.SendRequest(request);

        return response.Entries.Count > 0
            ? response.Entries[0]
            : null;
    }

    private static HashSet<int> LoadUsedGids(
        LdapConnection connection)
    {
        var request =
            new SearchRequest(
                "ou=Groups,dc=basarnas,dc=go,dc=id",
                "(objectClass=posixGroup)",
                SearchScope.OneLevel,
                "gidNumber");

        var response =
            (SearchResponse)connection.SendRequest(request);

        var usedGids =
            new HashSet<int>();

        foreach (SearchResultEntry entry in response.Entries)
        {
            if (entry.Attributes["gidNumber"] is null)
                continue;

            foreach (var value in entry.Attributes["gidNumber"])
            {
                if (int.TryParse(
                    value?.ToString(),
                    out var gid))
                {
                    usedGids.Add(gid);
                }
            }
        }

        return usedGids;
    }

    private static int GenerateGidNumber(
        HashSet<int> usedGids)
    {
        const int minimumGid = 11001;

        var candidate = minimumGid;

        while (usedGids.Contains(candidate))
            candidate++;

        return candidate;
    }

    private static string NormalizeGroupName(
        string name,
        string code)
    {
        var value =
            !string.IsNullOrWhiteSpace(name)
                ? name
                : code;

        return value
            .Trim()
            .Replace(" ", "_");
    }

    private static string EscapeFilter(
        string value)
    {
        return value
            .Replace("\\", "\\5c")
            .Replace("*", "\\2a")
            .Replace("(", "\\28")
            .Replace(")", "\\29")
            .Replace("\0", "\\00");
    }

    private static string EscapeDn(
        string value)
    {
        return value
            .Replace("\\", "\\5c")
            .Replace(",", "\\,")
            .Replace("+", "\\+")
            .Replace("\"", "\\\"")
            .Replace("<", "\\<")
            .Replace(">", "\\>")
            .Replace(";", "\\;")
            .Replace("#", "\\#");
    }
}
