using System.DirectoryServices.Protocols;

using BDIP.Application.Common;
using BDIP.Application.Locations;
using BDIP.Contracts.Locations;
using BDIP.Infrastructure.LDAP;

using Microsoft.Extensions.Options;

namespace BDIP.Infrastructure.Locations;

public class LdapLocationService : ILocationService
{
    private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Kantor Pusat",
            "Balai Diklat",
            "UPT"
        };

    private readonly ILdapConnectionFactory _factory;
    private readonly LdapOptions _options;

    public LdapLocationService(
        ILdapConnectionFactory factory,
        IOptions<LdapOptions> options)
    {
        _factory = factory;
        _options = options.Value;
    }

    private string LocationsDn =>
        $"ou=Locations,{_options.BaseDn}";

    private string UnitsDn =>
        $"ou=Units,{_options.BaseDn}";

    public Task<List<LocationResponse>> GetAllAsync()
    {
        using var connection = _factory.Create();

        var request = new SearchRequest(
            LocationsDn,
            "(objectClass=organizationalRole)",
            SearchScope.OneLevel,
            "cn",
            "description",
            "businessCategory");

        var response =
            (SearchResponse)connection.SendRequest(
                request);

        foreach (SearchResultEntry entry in response.Entries)
        {
            Console.WriteLine(
                $"LDAP Location=[{GetAttribute(entry, "cn")}]");
        }

        var locations = response.Entries
            .Cast<SearchResultEntry>()
            .Select(entry =>
                MapLocation(
                    connection,
                    entry))
            .OrderBy(location => location.Name)
            .ToList();

        return Task.FromResult(locations);
    }

    public Task<LocationResponse?> GetByNameAsync(
        string name)
    {
        var normalizedName = NormalizeName(name);

        using var connection = _factory.Create();

        var entry = FindLocationEntry(
            connection,
            normalizedName);

        if (entry is null)
            return Task.FromResult<LocationResponse?>(null);

        return Task.FromResult<LocationResponse?>(
            MapLocation(
                connection,
                entry));
    }

    public Task<LocationResponse> CreateAsync(
        CreateLocationRequest request)
    {
        var name = NormalizeName(request.Name);
        var description =
            NormalizeDescription(request.Description);
        var type = NormalizeType(request.Type);

        using var connection = _factory.Create();

        if (FindLocationEntry(connection, name) is not null)
        {
            throw new InvalidOperationException(
                $"Location '{name}' already exists.");
        }

        var locationDn =
            BuildLocationDn(name);

        var addRequest =
            new AddRequest(
                locationDn,
                new DirectoryAttribute(
                    "objectClass",
                    "top",
                    "organizationalRole"),
                new DirectoryAttribute(
                    "cn",
                    name),
                new DirectoryAttribute(
                    "businessCategory",
                    type));

        if (!string.IsNullOrWhiteSpace(description))
        {
            addRequest.Attributes.Add(
                new DirectoryAttribute(
                    "description",
                    description));
        }

        connection.SendRequest(addRequest);

        var created =
            FindLocationEntry(
                connection,
                name)
            ?? throw new InvalidOperationException(
                $"Location '{name}' was not found after create.");

        return Task.FromResult(
            MapLocation(
                connection,
                created));
    }

    public Task<LocationResponse> UpdateAsync(
        string currentName,
        UpdateLocationRequest request)
    {
        var normalizedCurrentName =
            NormalizeName(currentName);
        var newName =
            NormalizeName(request.Name);
        var description =
            NormalizeDescription(request.Description);
        var type =
            NormalizeType(request.Type);

        using var connection = _factory.Create();

        var currentEntry =
            FindLocationEntry(
                connection,
                normalizedCurrentName);

        if (currentEntry is null)
        {
            throw new InvalidOperationException(
                $"Location '{normalizedCurrentName}' not found.");
        }

        var oldDn =
            currentEntry.DistinguishedName;

        var newDn =
            BuildLocationDn(newName);

        if (!string.Equals(
                normalizedCurrentName,
                newName,
                StringComparison.OrdinalIgnoreCase))
        {
            var collision =
                FindLocationEntry(
                    connection,
                    newName);

            if (collision is not null)
            {
                throw new InvalidOperationException(
                    $"Location '{newName}' already exists.");
            }
        }

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

        var typeModification =
            new DirectoryAttributeModification
            {
                Name = "businessCategory",
                Operation =
                    DirectoryAttributeOperation.Replace
            };

        typeModification.Add(type);

        modifyRequest.Modifications.Add(
            typeModification);

        connection.SendRequest(modifyRequest);

        if (!string.Equals(
                normalizedCurrentName,
                newName,
                StringComparison.OrdinalIgnoreCase))
        {
            var renameRequest =
                new ModifyDNRequest(
                    oldDn,
                    LocationsDn,
                    $"cn={EscapeDn(newName)}");

            connection.SendRequest(renameRequest);

            UpdateUnitLocationReferences(
                connection,
                oldDn,
                newDn);
        }

        var updated =
            FindLocationEntry(
                connection,
                newName)
            ?? throw new InvalidOperationException(
                $"Location '{newName}' was not found after update.");

        return Task.FromResult(
            MapLocation(
                connection,
                updated));
    }

    public Task DeleteAsync(
        string name)
    {
        var normalizedName =
            NormalizeName(name);

        using var connection = _factory.Create();

        var entry =
            FindLocationEntry(
                connection,
                normalizedName);

        if (entry is null)
        {
            throw new InvalidOperationException(
                $"Location '{normalizedName}' not found.");
        }

        var unitCount =
            CountUnits(
                connection,
                entry.DistinguishedName);

        if (unitCount > 0)
        {
            throw new InvalidOperationException(
                $"Location '{normalizedName}' cannot be deleted while units are assigned.");
        }

        connection.SendRequest(
            new DeleteRequest(
                entry.DistinguishedName));

        return Task.CompletedTask;
    }

    private SearchResultEntry? FindLocationEntry(
        LdapConnection connection,
        string name)
    {
        var request = new SearchRequest(
            LocationsDn,
            $"(&(objectClass=organizationalRole)(cn={EscapeFilter(name)}))",
            SearchScope.OneLevel,
            "cn",
            "description",
            "businessCategory");

        var response =
            (SearchResponse)connection.SendRequest(
                request);

        if (response.Entries.Count == 0)
            return null;

        return response.Entries[0];
    }

    private LocationResponse MapLocation(
        LdapConnection connection,
        SearchResultEntry entry)
    {
        return new LocationResponse
        {
            Name =
                GetAttribute(entry, "cn"),
            Description =
                GetAttribute(entry, "description"),
            Type =
                GetAttribute(entry, "businessCategory"),
            UnitCount =
                CountUnits(
                    connection,
                    entry.DistinguishedName)
        };
    }

    private int CountUnits(
        LdapConnection connection,
        string locationDn)
    {
        var request = new SearchRequest(
            UnitsDn,
            $"(&(objectClass=organizationalRole)(seeAlso={EscapeFilter(locationDn)}))",
            SearchScope.OneLevel,
            "cn");

        var response =
            (SearchResponse)connection.SendRequest(
                request);

        return response.Entries.Count;
    }

    private void UpdateUnitLocationReferences(
        LdapConnection connection,
        string oldLocationDn,
        string newLocationDn)
    {
        var request = new SearchRequest(
            UnitsDn,
            $"(&(objectClass=organizationalRole)(seeAlso={EscapeFilter(oldLocationDn)}))",
            SearchScope.OneLevel,
            "cn",
            "seeAlso");

        var response =
            (SearchResponse)connection.SendRequest(
                request);

        foreach (SearchResultEntry entry in response.Entries)
        {
            var modification =
                new DirectoryAttributeModification
                {
                    Name = "seeAlso",
                    Operation =
                        DirectoryAttributeOperation.Replace
                };

            modification.Add(newLocationDn);

            var modifyRequest =
                new ModifyRequest(
                    entry.DistinguishedName,
                    modification);

            connection.SendRequest(modifyRequest);
        }
    }

    private string BuildLocationDn(
        string name)
    {
        return
            $"cn={EscapeDn(name)},{LocationsDn}";
    }

    private static string NormalizeName(
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

    private static string NormalizeDescription(
        string value)
    {
        return value?.Trim() ?? "";
    }

    private static string NormalizeType(
        string value)
    {
        var normalized =
            value?.Trim() ?? "";

        var canonical =
            AllowedTypes.FirstOrDefault(
                type =>
                    string.Equals(
                        type,
                        normalized,
                        StringComparison.OrdinalIgnoreCase));

        if (canonical is null)
        {
            throw new ArgumentException(
                "Location type must be one of: Kantor Pusat, Balai Diklat, UPT.");
        }

        return canonical;
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
