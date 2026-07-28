using BDIP.Application.Groups;
using BDIP.Application.Roles;
using BDIP.Contracts.Roles;
using BDIP.Infrastructure.LDAP;

using System.DirectoryServices.Protocols;

using Microsoft.Extensions.Options;

namespace BDIP.Infrastructure.Roles;

public sealed class LdapRoleService : IRoleService
{
    private readonly ILdapConnectionFactory _factory;
    private readonly LdapOptions _options;
    private readonly IUserDnResolver _userDnResolver;

    private string RolesDn =>
        $"ou=Roles,{_options.BaseDn}";

    private string PlaceholderMemberDn =>
        _options.PlaceholderMemberDn;

    public LdapRoleService(
        ILdapConnectionFactory factory,
        IOptions<LdapOptions> options,
        IUserDnResolver userDnResolver)
    {
        _factory = factory;
        _options = options.Value;
        _userDnResolver = userDnResolver;
    }

    public Task<IReadOnlyList<RoleResponse>> GetAllAsync()
    {
        using var connection = _factory.Create();

        var request = new SearchRequest(
            RolesDn,
            "(objectClass=groupOfNames)",
            SearchScope.OneLevel,
            "cn",
            "description",
            "member");

        var response =
            (SearchResponse)connection.SendRequest(request);

        IReadOnlyList<RoleResponse> roles =
            response.Entries
                .Cast<SearchResultEntry>()
                .Select(Map)
                .OrderBy(x => x.Name)
                .ToList();

        return Task.FromResult(roles);
    }

    public Task<RoleResponse?> GetByNameAsync(
        string name)
    {
        var normalizedName = NormalizeName(name);

        using var connection = _factory.Create();

        var request = new SearchRequest(
            RolesDn,
            $"(&(objectClass=groupOfNames)(cn={EscapeFilter(normalizedName)}))",
            SearchScope.OneLevel,
            "cn",
            "description",
            "member");

        var response =
            (SearchResponse)connection.SendRequest(request);

        if (response.Entries.Count == 0)
        {
            return Task.FromResult<RoleResponse?>(null);
        }

        return Task.FromResult<RoleResponse?>(
            Map(response.Entries[0]));
    }

    public Task<RoleMembersResponse?> GetMembersAsync(
        string name)
    {
        var normalizedName = NormalizeName(name);

        using var connection = _factory.Create();

        var roleRequest = new SearchRequest(
            RolesDn,
            $"(&(objectClass=groupOfNames)(cn={EscapeFilter(normalizedName)}))",
            SearchScope.OneLevel,
            "cn",
            "member");

        var roleResponse =
            (SearchResponse)connection.SendRequest(
                roleRequest);

        if (roleResponse.Entries.Count == 0)
        {
            return Task.FromResult<RoleMembersResponse?>(
                null);
        }

        var role = roleResponse.Entries[0];

        var result = new RoleMembersResponse
        {
            RoleName =
                GetAttribute(role, "cn")
        };

        var members =
            role.Attributes["member"];

        if (members is null)
        {
            return Task.FromResult<RoleMembersResponse?>(
                result);
        }

        for (var i = 0; i < members.Count; i++)
        {
            var memberDn =
                members[i]?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(memberDn))
            {
                continue;
            }

            if (string.Equals(
                memberDn,
                PlaceholderMemberDn,
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
                    "uid",
                    "cn",
                    "mail",
                    "ou");

                var userResponse =
                    (SearchResponse)connection.SendRequest(
                        userRequest);

                if (userResponse.Entries.Count == 0)
                {
                    continue;
                }

                var user = userResponse.Entries[0];

                result.Members.Add(
                    new RoleMemberResponse
                    {
                        Username =
                            GetAttribute(user, "uid"),

                        FullName =
                            GetAttribute(user, "cn"),

                        Email =
                            GetAttribute(user, "mail"),

                        Unit =
                            GetAttribute(user, "ou"),

                        DistinguishedName =
                            user.DistinguishedName
                    });
            }
            catch (DirectoryOperationException)
            {
                // Stale member DN must not prevent
                // other role members from being returned.
            }
        }

        return Task.FromResult<RoleMembersResponse?>(
            result);
    }

    public Task<RoleResponse> CreateAsync(
        CreateRoleRequest request)
    {
        var name = NormalizeName(request.Name);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Role name is required.");
        }

        using var connection = _factory.Create();

        if (Exists(connection, name))
        {
            throw new InvalidOperationException(
                $"Role '{name}' already exists.");
        }

        var addRequest = new AddRequest(
            $"cn={EscapeDn(name)},{RolesDn}",
            new DirectoryAttribute(
                "objectClass",
                "top",
                "groupOfNames"),
            new DirectoryAttribute(
                "cn",
                name),
            new DirectoryAttribute(
                "member",
                PlaceholderMemberDn));

        var description =
            request.Description?.Trim() ?? "";

        if (!string.IsNullOrWhiteSpace(description))
        {
            addRequest.Attributes.Add(
                new DirectoryAttribute(
                    "description",
                    description));
        }

        connection.SendRequest(addRequest);

        return Task.FromResult(
            new RoleResponse
            {
                Name = name,
                Description = description,
                MemberCount = 0
            });
    }

    public Task<RoleResponse?> UpdateAsync(
        string name,
        UpdateRoleRequest request)
    {
        var currentName = NormalizeName(name);
        var newName = NormalizeName(request.Name);

        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException(
                "Role name is required.");
        }

        using var connection = _factory.Create();

        if (!Exists(connection, currentName))
        {
            return Task.FromResult<RoleResponse?>(null);
        }

        if (!string.Equals(
            currentName,
            newName,
            StringComparison.OrdinalIgnoreCase))
        {
            if (Exists(connection, newName))
            {
                throw new InvalidOperationException(
                    $"Role '{newName}' already exists.");
            }

            var modifyDnRequest = new ModifyDNRequest(
                $"cn={EscapeDn(currentName)},{RolesDn}",
                RolesDn,
                $"cn={EscapeRdn(newName)}");

            modifyDnRequest.DeleteOldRdn = true;

            connection.SendRequest(modifyDnRequest);
        }

        var description =
            request.Description?.Trim() ?? "";

        var modification =
            new DirectoryAttributeModification
            {
                Name = "description",
                Operation =
                    DirectoryAttributeOperation.Replace
            };

        if (!string.IsNullOrWhiteSpace(description))
        {
            modification.Add(description);
        }

        connection.SendRequest(
            new ModifyRequest(
                $"cn={EscapeDn(newName)},{RolesDn}",
                modification));

        return Task.FromResult<RoleResponse?>(
            GetByNameInternal(
                connection,
                newName));
    }

    public async Task AddMemberAsync(
        string name,
        string username)
    {
        var normalizedName = NormalizeName(name);

        var userDn =
            await _userDnResolver.GetUserDnAsync(
                username);

        if (string.IsNullOrWhiteSpace(userDn))
        {
            throw new InvalidOperationException(
                $"User '{username}' not found.");
        }

        using var connection = _factory.Create();

        var roleRequest = new SearchRequest(
            RolesDn,
            $"(&(objectClass=groupOfNames)(cn={EscapeFilter(normalizedName)}))",
            SearchScope.OneLevel,
            "cn",
            "member");

        var roleResponse =
            (SearchResponse)connection.SendRequest(
                roleRequest);

        if (roleResponse.Entries.Count == 0)
        {
            throw new InvalidOperationException(
                $"Role '{normalizedName}' not found.");
        }

        var role = roleResponse.Entries[0];

        var memberDns =
            role.Attributes["member"]?
                .GetValues(typeof(string))
                .Cast<string>()
                .ToList()
            ?? new List<string>();

        if (memberDns.Any(x =>
            string.Equals(
                x,
                userDn,
                StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var request =
            new ModifyRequest(
                role.DistinguishedName);

        var addMember =
            new DirectoryAttributeModification
            {
                Name = "member",
                Operation =
                    DirectoryAttributeOperation.Add
            };

        addMember.Add(userDn);

        request.Modifications.Add(
            addMember);

        if (memberDns.Any(x =>
            string.Equals(
                x,
                PlaceholderMemberDn,
                StringComparison.OrdinalIgnoreCase)))
        {
            var removePlaceholder =
                new DirectoryAttributeModification
                {
                    Name = "member",
                    Operation =
                        DirectoryAttributeOperation.Delete
                };

            removePlaceholder.Add(
                PlaceholderMemberDn);

            request.Modifications.Add(
                removePlaceholder);
        }

        connection.SendRequest(request);
    }

    public async Task RemoveMemberAsync(
        string name,
        string username)
    {
        var normalizedName = NormalizeName(name);

        var userDn =
            await _userDnResolver.GetUserDnAsync(
                username);

        if (string.IsNullOrWhiteSpace(userDn))
        {
            throw new InvalidOperationException(
                $"User '{username}' not found.");
        }

        using var connection = _factory.Create();

        var roleRequest = new SearchRequest(
            RolesDn,
            $"(&(objectClass=groupOfNames)(cn={EscapeFilter(normalizedName)}))",
            SearchScope.OneLevel,
            "cn",
            "member");

        var roleResponse =
            (SearchResponse)connection.SendRequest(
                roleRequest);

        if (roleResponse.Entries.Count == 0)
        {
            throw new InvalidOperationException(
                $"Role '{normalizedName}' not found.");
        }

        var role = roleResponse.Entries[0];

        var memberDns =
            role.Attributes["member"]?
                .GetValues(typeof(string))
                .Cast<string>()
                .ToList()
            ?? new List<string>();

        var isMember =
            memberDns.Any(x =>
                string.Equals(
                    x,
                    userDn,
                    StringComparison.OrdinalIgnoreCase));

        if (!isMember)
        {
            return;
        }

        var realMemberCount =
            memberDns.Count(x =>
                !string.Equals(
                    x,
                    PlaceholderMemberDn,
                    StringComparison.OrdinalIgnoreCase));

        var request =
            new ModifyRequest(
                role.DistinguishedName);

        if (realMemberCount == 1)
        {
            var addPlaceholder =
                new DirectoryAttributeModification
                {
                    Name = "member",
                    Operation =
                        DirectoryAttributeOperation.Add
                };

            addPlaceholder.Add(
                PlaceholderMemberDn);

            request.Modifications.Add(
                addPlaceholder);
        }

        var removeMember =
            new DirectoryAttributeModification
            {
                Name = "member",
                Operation =
                    DirectoryAttributeOperation.Delete
            };

        removeMember.Add(userDn);

        request.Modifications.Add(
            removeMember);

        connection.SendRequest(request);
    }

    public Task<bool> DeleteAsync(
        string name)
    {
        var normalizedName = NormalizeName(name);

        using var connection = _factory.Create();

        var role =
            GetByNameInternal(
                connection,
                normalizedName);

        if (role is null)
        {
            return Task.FromResult(false);
        }

        if (role.MemberCount > 0)
        {
            throw new InvalidOperationException(
                "Role cannot be deleted while users are assigned.");
        }

        connection.SendRequest(
            new DeleteRequest(
                $"cn={EscapeDn(normalizedName)},{RolesDn}"));

        return Task.FromResult(true);
    }

    private RoleResponse? GetByNameInternal(
        LdapConnection connection,
        string name)
    {
        var request = new SearchRequest(
            RolesDn,
            $"(&(objectClass=groupOfNames)(cn={EscapeFilter(name)}))",
            SearchScope.OneLevel,
            "cn",
            "description",
            "member");

        var response =
            (SearchResponse)connection.SendRequest(request);

        return response.Entries.Count == 0
            ? null
            : Map(response.Entries[0]);
    }

    private bool Exists(
        LdapConnection connection,
        string name)
    {
        var request = new SearchRequest(
            RolesDn,
            $"(&(objectClass=groupOfNames)(cn={EscapeFilter(name)}))",
            SearchScope.OneLevel,
            "cn");

        var response =
            (SearchResponse)connection.SendRequest(request);

        return response.Entries.Count > 0;
    }

    private RoleResponse Map(
        SearchResultEntry entry)
    {
        var members =
            entry.Attributes["member"];

        var memberCount =
            members is null
                ? 0
                : members
                    .GetValues(typeof(string))
                    .Cast<string>()
                    .Count(x =>
                        !string.Equals(
                            x,
                            PlaceholderMemberDn,
                            StringComparison.OrdinalIgnoreCase));

        return new RoleResponse
        {
            Name = GetAttribute(entry, "cn"),
            Description =
                GetAttribute(entry, "description"),
            MemberCount = memberCount
        };
    }

    private static string GetAttribute(
        SearchResultEntry entry,
        string name)
    {
        var attribute = entry.Attributes[name];

        if (attribute is null ||
            attribute.Count == 0)
        {
            return "";
        }

        return attribute[0]?.ToString() ?? "";
    }

    private static string NormalizeName(
        string? value)
    {
        return value?.Trim() ?? "";
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
            .Replace("\\", "\\\\")
            .Replace(",", "\\,")
            .Replace("+", "\\+")
            .Replace("\"", "\\\"")
            .Replace("<", "\\<")
            .Replace(">", "\\>")
            .Replace(";", "\\;");
    }

    private static string EscapeRdn(
        string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace(",", "\\,")
            .Replace("+", "\\+")
            .Replace("\"", "\\\"")
            .Replace("<", "\\<")
            .Replace(">", "\\>")
            .Replace(";", "\\;");
    }
}
