using System.DirectoryServices.Protocols;
using BDIP.Application.Groups;
using BDIP.Domain.Entities;
using Microsoft.Extensions.Options;

namespace BDIP.Infrastructure.LDAP.Repository;

public class LdapGroupRepository : IGroupRepository
{
    private readonly ILdapConnectionFactory _connectionFactory;
    private readonly LdapOptions _options;

    public LdapGroupRepository(
        ILdapConnectionFactory connectionFactory,
        IOptions<LdapOptions> options)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
    }

    #region Query

    public async Task<bool> ExistsAsync(string groupName)
    {
        using var connection = _connectionFactory.Create();

        var request = new SearchRequest(
            _options.GroupsDn,
            $"(cn={Escape(groupName)})",
            SearchScope.OneLevel);

        var response =
            (SearchResponse)await Task.Run(() =>
                connection.SendRequest(request));

        return response.Entries.Count > 0;
    }

    public async Task<Group?> GetByNameAsync(string groupName)
    {
        using var connection = _connectionFactory.Create();

        var request = new SearchRequest(
            _options.GroupsDn,
            $"(cn={Escape(groupName)})",
            SearchScope.OneLevel);

        var response =
            (SearchResponse)await Task.Run(() =>
                connection.SendRequest(request));

        if (response.Entries.Count == 0)
            return null;

        return Map(response.Entries[0]);
    }

    public async Task<IReadOnlyList<Group>> GetAllAsync()
    {
        using var connection = _connectionFactory.Create();

        var request = new SearchRequest(
            _options.GroupsDn,
            "(objectClass=groupOfNames)",
            SearchScope.OneLevel);

        var response =
            (SearchResponse)await Task.Run(() =>
                connection.SendRequest(request));

        var result = new List<Group>();

        foreach (SearchResultEntry entry in response.Entries)
        {
            result.Add(Map(entry));
        }

        return result;
    }

    #endregion

    #region Mapper

    private static Group Map(SearchResultEntry entry)
    {
        var group = new Group();

        group.DistinguishedName = entry.DistinguishedName;

        if (entry.Attributes["cn"] != null)
            group.Name =
                entry.Attributes["cn"][0]?.ToString() ?? "";

        if (entry.Attributes["description"] != null)
            group.Description =
                entry.Attributes["description"][0]?.ToString();

        if (entry.Attributes["gidNumber"] != null)
        {
            int.TryParse(
                entry.Attributes["gidNumber"][0]?.ToString(),
                out var gid);

            group.GidNumber = gid;
        }

        if (entry.Attributes["member"] != null)
        {
            foreach (var member in entry.Attributes["member"])
            {
                if (member != null)
                    group.Members.Add(member.ToString()!);
            }
        }

        if (entry.Attributes["objectClass"] != null)
        {
            group.ObjectClasses.Clear();

            foreach (var objectClass in entry.Attributes["objectClass"])
            {
                if (objectClass != null)
                    group.ObjectClasses.Add(objectClass.ToString()!);
            }
        }

        return group;
    }

        #endregion

    #region Helper

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\5c")
            .Replace("*", "\\2a")
            .Replace("(", "\\28")
            .Replace(")", "\\29")
            .Replace("\0", "\\00");
    }

    #endregion

    // ===========================
    // PART 2
    // ===========================
            #region Command

    public async Task<bool> ExistsAsync(
        string groupName,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();

        var request = new SearchRequest(
            _options.GroupsDn,
            $"(cn={Escape(groupName)})",
            SearchScope.OneLevel,
            "cn");

        var response =
            (SearchResponse)await Task.Run(
                () => connection.SendRequest(request),
                cancellationToken);

        return response.Entries.Count > 0;
    }

    public async Task<Group> CreateAsync(
        Group group,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();

        var dn =
            $"cn={Escape(group.Name)},{_options.GroupsDn}";

        var request = new AddRequest(dn);

        // ====================================================
        // IMPORTANT
        // ====================================================
        // OpenLDAP pada server BDIP menggunakan schema dimana
        // groupOfNames dan posixGroup sama-sama STRUCTURAL.
        //
        // Karena itu TIDAK BOLEH digunakan bersamaan.
        //
        // BDIP Phase-1 menggunakan groupOfNames.
        // ====================================================

        request.Attributes.Add(
            new DirectoryAttribute(
                "objectClass",
                "top",
                "groupOfNames"));

        request.Attributes.Add(
            new DirectoryAttribute(
                "cn",
                group.Name));

        if (!string.IsNullOrWhiteSpace(group.Description))
        {
            request.Attributes.Add(
                new DirectoryAttribute(
                    "description",
                    group.Description));
        }

        if (group.Members.Count == 0)
        {
            request.Attributes.Add(
                new DirectoryAttribute(
                    "member",
                    _options.PlaceholderMemberDn));
        }
        else
        {
            request.Attributes.Add(
                new DirectoryAttribute(
                    "member",
                    group.Members.ToArray()));
        }

        await Task.Run(
            () => connection.SendRequest(request),
            cancellationToken);

        group.DistinguishedName = dn;
        group.CreatedAt = DateTime.UtcNow;

        return group;
    }

    public async Task<Group> UpdateAsync(
        Group group,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();

        var request =
            new ModifyRequest(group.DistinguishedName);

        var descriptionModification =
            new DirectoryAttributeModification
            {
                Name = "description",
                Operation = DirectoryAttributeOperation.Replace
            };

        if (!string.IsNullOrWhiteSpace(group.Description))
        {
            descriptionModification.Add(
                group.Description);
        }

        request.Modifications.Add(
            descriptionModification);

        await Task.Run(
            () => connection.SendRequest(request),
            cancellationToken);

        return group;
    }

    #endregion


    // ===========================
    // PART 3
    // ===========================
    

        public async Task DeleteAsync(
        string groupName,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();

        var group = await GetByNameAsync(
            groupName);

        if (group is null)
            throw new InvalidOperationException(
                $"Group '{groupName}' tidak ditemukan.");

        var request =
            new DeleteRequest(group.DistinguishedName);

        await Task.Run(
            () => connection.SendRequest(request),
            cancellationToken);
    }

    public async Task AddMemberAsync(
        string groupName,
        string memberDn,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();

        var group = await GetByNameAsync(
            groupName);

        if (group is null)
            throw new InvalidOperationException(
                $"Group '{groupName}' tidak ditemukan.");

        var modification =
            new DirectoryAttributeModification
            {
                Name = "member",
                Operation = DirectoryAttributeOperation.Add
            };

        modification.Add(memberDn);

        var request =
            new ModifyRequest(group.DistinguishedName);

        request.Modifications.Add(modification);

        await Task.Run(
            () => connection.SendRequest(request),
            cancellationToken);
    }

    public async Task RemoveMemberAsync(
        string groupName,
        string memberDn,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();

        var group = await GetByNameAsync(
            groupName);

        if (group is null)
            throw new InvalidOperationException(
                $"Group '{groupName}' tidak ditemukan.");

        var modification =
            new DirectoryAttributeModification
            {
                Name = "member",
                Operation = DirectoryAttributeOperation.Delete
            };

        modification.Add(memberDn);

        var request =
            new ModifyRequest(group.DistinguishedName);

        request.Modifications.Add(modification);

        await Task.Run(
            () => connection.SendRequest(request),
            cancellationToken);
    }
}
