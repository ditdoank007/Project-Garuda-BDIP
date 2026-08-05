using System.DirectoryServices.Protocols;

using BDIP.Application.Units;
using BDIP.Contracts.Units;
using BDIP.Infrastructure.LDAP;

using Microsoft.Extensions.Options;

namespace BDIP.Infrastructure.Units;

public class LdapUnitService : IUnitService
{
    private readonly ILdapConnectionFactory _ldap;
    private readonly LdapOptions _options;

    public LdapUnitService(
        ILdapConnectionFactory ldap,
        IOptions<LdapOptions> options)
    {
        _ldap = ldap;
        _options = options.Value;
    }

    private string UnitsDn =>
        $"ou=Units,{_options.BaseDn}";

    private string PeopleDn =>
        $"ou=People,{_options.BaseDn}";

    private string LocationsDn =>
        $"ou=Locations,{_options.BaseDn}";

    public async Task<List<UnitResponse>> GetAllAsync()
    {
        using var connection = _ldap.Create();

        var request = new SearchRequest(
            UnitsDn,
            "(objectClass=organizationalRole)",
            SearchScope.OneLevel,
            "cn",
            "description",
            "seeAlso");

        var response =
            (SearchResponse)connection.SendRequest(
                request);

        var units = new List<UnitResponse>();

        foreach (SearchResultEntry entry in response.Entries)
        {
            var name =
                GetAttribute(entry, "cn");

            if (string.IsNullOrWhiteSpace(name))
                continue;

            var locationDn =
                GetAttribute(entry, "seeAlso");

            units.Add(new UnitResponse
            {
                Name = name,
                Description =
                    GetAttribute(entry, "description"),
                LocationDn = locationDn,
                LocationName =
                    GetLocationName(
                        connection,
                        locationDn),
                UserCount =
                    await CountUsersAsync(name)
            });
        }

        return units
            .OrderBy(x => x.Name)
            .ToList();
    }

    public async Task<UnitResponse?> GetByNameAsync(
        string name)
    {
        var normalizedName =
            NormalizeName(name);

        using var connection = _ldap.Create();

        var entry =
            FindUnitEntry(
                connection,
                normalizedName);

        if (entry is null)
            return null;

        var locationDn =
            GetAttribute(entry, "seeAlso");

        return new UnitResponse
        {
            Name =
                GetAttribute(entry, "cn"),
            Description =
                GetAttribute(entry, "description"),
            LocationDn =
                locationDn,
            LocationName =
                GetLocationName(
                    connection,
                    locationDn),
            UserCount =
                await CountUsersAsync(
                    GetAttribute(entry, "cn"))
        };
    }

    public async Task<UnitResponse> CreateAsync(
        CreateUnitRequest request)
    {
        var name =
            NormalizeName(request.Name);
        var description =
            NormalizeDescription(request.Description);
        var locationName =
            NormalizeLocationName(request.LocationName);

        using var connection = _ldap.Create();

        if (FindUnitEntry(connection, name) is not null)
        {
            throw new InvalidOperationException(
                $"Unit '{name}' already exists.");
        }

        Console.WriteLine("========== CREATE UNIT ==========");
        Console.WriteLine($"Name=[{name}]");
        Console.WriteLine($"Location=[{locationName}]");

        var locationEntry =
            FindLocationEntry(
                connection,
                locationName);

        Console.WriteLine(
            $"Location found = {locationEntry != null}");

        if (locationEntry is null)
        {
            throw new InvalidOperationException(
                $"Location '{locationName}' not found.");
        }

        var unitDn =
            BuildUnitDn(name);

        var addRequest =
            new AddRequest(
                unitDn,
                new DirectoryAttribute(
                    "objectClass",
                    "top",
                    "organizationalRole"),
                new DirectoryAttribute(
                    "cn",
                    name),
                new DirectoryAttribute(
                    "seeAlso",
                    locationEntry.DistinguishedName));

        if (!string.IsNullOrWhiteSpace(description))
        {
            addRequest.Attributes.Add(
                new DirectoryAttribute(
                    "description",
                    description));
        }

        connection.SendRequest(addRequest);

        return
            await GetByNameAsync(name)
            ?? throw new InvalidOperationException(
                $"Unit '{name}' was not found after create.");
    }

    public async Task<UnitResponse> UpdateAsync(
        string currentName,
        UpdateUnitRequest request)
    {
        var normalizedCurrentName =
            NormalizeName(currentName);
        var newName =
            NormalizeName(request.Name);
        var description =
            NormalizeDescription(request.Description);
        var locationName =
            NormalizeLocationName(request.LocationName);

        using var connection = _ldap.Create();

        var currentEntry =
            FindUnitEntry(
                connection,
                normalizedCurrentName);

        if (currentEntry is null)
            return null;

        var locationEntry =
            FindLocationEntry(
                connection,
                locationName)
            ?? throw new InvalidOperationException(
                $"Location '{locationName}' not found.");


        if (!string.Equals(
                normalizedCurrentName,
                newName,
                StringComparison.OrdinalIgnoreCase))
        {
            var collision =
                FindUnitEntry(
                    connection,
                    newName);

            if (collision is not null)
            {
                throw new InvalidOperationException(
                    $"Unit '{newName}' already exists.");
            }
        }

        var oldDn =
            currentEntry.DistinguishedName;

        var modifyRequest =
            new ModifyRequest(oldDn);

        var descriptionModification =
            new DirectoryAttributeModification
            {
                Name = "description",
                Operation =
                    DirectoryAttributeOperation.Replace
            };

        if (!string.IsNullOrWhiteSpace(description))
        {
            descriptionModification.Add(description);
        }

        modifyRequest.Modifications.Add(
            descriptionModification);

        var locationModification =
            new DirectoryAttributeModification
            {
                Name = "seeAlso",
                Operation =
                    DirectoryAttributeOperation.Replace
            };

        locationModification.Add(
            locationEntry.DistinguishedName);

        modifyRequest.Modifications.Add(
            locationModification);

        connection.SendRequest(modifyRequest);

        if (!string.Equals(
                normalizedCurrentName,
                newName,
                StringComparison.OrdinalIgnoreCase))
        {
            var renameRequest =
                new ModifyDNRequest(
                    oldDn,
                    UnitsDn,
                    $"cn={EscapeDn(newName)}");

            connection.SendRequest(renameRequest);

            UpdateUserUnitReferences(
                connection,
                normalizedCurrentName,
                newName);
        }

        return await GetByNameAsync(newName);
    }

    public Task DeleteAsync(
        string name)
    {
        var normalizedName =
            NormalizeName(name);

        using var connection = _ldap.Create();

        var entry =
            FindUnitEntry(
                connection,
                normalizedName);

        if (entry is null)
            return Task.FromResult(false);

        var userCount =
            CountUsers(
                connection,
                normalizedName);

        if (userCount > 0)
        {
            throw new InvalidOperationException(
                $"Unit '{normalizedName}' cannot be deleted while users are assigned.");
        }

        connection.SendRequest(
            new DeleteRequest(
                entry.DistinguishedName));

        return Task.FromResult(true);
    }

    private SearchResultEntry? FindUnitEntry(
        LdapConnection connection,
        string name)
    {
        var request = new SearchRequest(
            UnitsDn,
            $"(&(objectClass=organizationalRole)(cn={EscapeFilter(name)}))",
            SearchScope.OneLevel,
            "cn",
            "description",
            "seeAlso");

        var response =
            (SearchResponse)connection.SendRequest(
                request);

        if (response.Entries.Count == 0)
            return null;

        return response.Entries[0];
    }

    private SearchResultEntry? FindLocationEntry(
        LdapConnection connection,
        string name)
    {
        var request = new SearchRequest(
            LocationsDn,
            $"(&(objectClass=organizationalRole)(cn={EscapeFilter(name)}))",
            SearchScope.OneLevel,
            "cn");

        var response =
            (SearchResponse)connection.SendRequest(
                request);

        if (response.Entries.Count == 0)
            return null;

        return response.Entries[0];
    }

    private string GetLocationName(
        LdapConnection connection,
        string locationDn)
    {
        if (string.IsNullOrWhiteSpace(locationDn))
            return "";

        try
        {
            var request = new SearchRequest(
                locationDn,
                "(objectClass=*)",
                SearchScope.Base,
                "cn");

            var response =
                (SearchResponse)connection.SendRequest(
                    request);

            if (response.Entries.Count == 0)
                return "";

            return GetAttribute(
                response.Entries[0],
                "cn");
        }
        catch (DirectoryOperationException)
        {
            return "";
        }
    }

    private Task<int> CountUsersAsync(
        string unitName)
    {
        using var connection = _ldap.Create();

        return Task.FromResult(
            CountUsers(
                connection,
                unitName));
    }

    private int CountUsers(
        LdapConnection connection,
        string unitName)
    {
        var request = new SearchRequest(
            PeopleDn,
            $"(&(objectClass=inetOrgPerson)(ou={EscapeFilter(unitName)}))",
            SearchScope.OneLevel,
            "uid");

        var response =
            (SearchResponse)connection.SendRequest(
                request);

        return response.Entries.Count;
    }

    private void UpdateUserUnitReferences(
        LdapConnection connection,
        string oldUnitName,
        string newUnitName)
    {
        var request = new SearchRequest(
            PeopleDn,
            $"(&(objectClass=inetOrgPerson)(ou={EscapeFilter(oldUnitName)}))",
            SearchScope.OneLevel,
            "uid",
            "ou");

        var response =
            (SearchResponse)connection.SendRequest(
                request);

        foreach (SearchResultEntry entry in response.Entries)
        {
            var modification =
                new DirectoryAttributeModification
                {
                    Name = "ou",
                    Operation =
                        DirectoryAttributeOperation.Replace
                };

            modification.Add(newUnitName);

            connection.SendRequest(
                new ModifyRequest(
                    entry.DistinguishedName,
                    modification));
        }
    }

    private string BuildUnitDn(
        string name)
    {
        return
            $"cn={EscapeDn(name)},{UnitsDn}";
    }

    private static string NormalizeName(
        string value)
    {
        var normalized =
            value?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                "Unit name is required.");
        }

        return normalized;
    }

    private static string NormalizeDescription(
        string value)
    {
        return value?.Trim() ?? "";
    }

    private static string NormalizeLocationName(
        string value)
    {
        var normalized =
            value?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                "Location name is required.");
        }

        return normalized;
    }

    private static string GetAttribute(
        SearchResultEntry entry,
        string attributeName)
    {
        if (!entry.Attributes.Contains(attributeName))
            return "";

        var attribute =
            entry.Attributes[attributeName];

        if (attribute is null ||
            attribute.Count == 0)
            return "";

        return attribute[0]?.ToString() ?? "";
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
}
