using System.DirectoryServices.Protocols;

using BDIP.Application.ImportUsers;
using BDIP.Application.Users.Import;
using BDIP.Contracts.Users.Import;
using BDIP.Infrastructure.LDAP;

namespace BDIP.Infrastructure.ImportUsers;

public class SynologyUserImportService : ISynologyUserImportService
{
    private readonly ISynologyUserCsvParser _parser;
    private readonly ILdapConnectionFactory _ldap;
    private readonly LdapOptions _options;

    public SynologyUserImportService(
        ISynologyUserCsvParser parser,
        ILdapConnectionFactory ldap,
        Microsoft.Extensions.Options.IOptions<LdapOptions> options)
    {
        _parser = parser;
        _ldap = ldap;
        _options = options.Value;
    }

    public async Task<ExecuteSynologyUserImportResponse> ExecuteAsync(
        ExecuteSynologyUserImportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CsvPath))
        {
            throw new ArgumentException(
                "CSV path is required.",
                nameof(request.CsvPath));
        }

        if (string.IsNullOrWhiteSpace(request.InitialPassword))
        {
            throw new ArgumentException(
                "Initial password is required.",
                nameof(request.InitialPassword));
        }

        if (!File.Exists(request.CsvPath))
        {
            throw new FileNotFoundException(
                "CSV file was not found.",
                request.CsvPath);
        }

        await using var csvStream = File.OpenRead(request.CsvPath);

        var preview = await _parser.PreviewAsync(csvStream);

        var result = new ExecuteSynologyUserImportResponse
        {
            TotalRows = preview.TotalRows
        };

        using var connection = _ldap.Create();

        var uidSearch = new SearchRequest(
            _options.PeopleDn,
            "(uidNumber=*)",
            SearchScope.Subtree,
            new[] { "uidNumber" });

        var uidResponse =
            (SearchResponse)connection.SendRequest(uidSearch);

        var nextUidNumber =
            LdapUidNumberGenerator.GetNext(uidResponse);

        foreach (var item in preview.Users)
        {
            if (item.Action != "Create")
            {
                result.SkippedExistingUsers++;
                continue;
            }

            try
            {
                var userDn =
                    $"uid={EscapeDnValue(item.Username)},{_options.PeopleDn}";

                var names = item.FullName.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

                var givenName = names.Length > 0
                    ? names[0]
                    : item.Username;

                var sn = names.Length > 1
                    ? names[^1]
                    : givenName;

                var addRequest = new AddRequest(userDn);

                addRequest.Attributes.Add(
                    new DirectoryAttribute(
                        "objectClass",
                        "top",
                        "person",
                        "organizationalPerson",
                        "inetOrgPerson",
                        "posixAccount",
                        "shadowAccount"));

                addRequest.Attributes.Add(
                    new DirectoryAttribute("uid", item.Username));

                addRequest.Attributes.Add(
                    new DirectoryAttribute("cn", item.FullName));

                addRequest.Attributes.Add(
                    new DirectoryAttribute("sn", sn));

                addRequest.Attributes.Add(
                    new DirectoryAttribute("givenName", givenName));

                addRequest.Attributes.Add(
                    new DirectoryAttribute(
                        "displayName",
                        item.FullName));

                if (!string.IsNullOrWhiteSpace(item.Email))
                {
                    addRequest.Attributes.Add(
                        new DirectoryAttribute("mail", item.Email));
                }

                addRequest.Attributes.Add(
                    new DirectoryAttribute(
                        "userPassword",
                        request.InitialPassword));

                addRequest.Attributes.Add(
                    new DirectoryAttribute(
                        "uidNumber",
                        nextUidNumber.ToString()));

                addRequest.Attributes.Add(
                    new DirectoryAttribute("gidNumber", "10000"));

                addRequest.Attributes.Add(
                    new DirectoryAttribute(
                        "homeDirectory",
                        $"/home/{item.Username}"));

                addRequest.Attributes.Add(
                    new DirectoryAttribute("loginShell", "/bin/bash"));

                if (!item.Enabled)
                {
                    addRequest.Attributes.Add(
                        new DirectoryAttribute("shadowExpire", "1"));
                }

                connection.SendRequest(addRequest);

                result.CreatedUsers++;
                nextUidNumber++;

                if (!item.Enabled)
                {
                    result.DisabledUsers++;
                }

                foreach (var sourceGroupName in item.Groups)
                {
                    var groupName = NormalizeGroupName(sourceGroupName);

                    var groupDn = FindGroupDn(
                        connection,
                        groupName);

                    if (string.IsNullOrWhiteSpace(groupDn))
                    {
                        result.Errors.Add(
                            $"User '{item.Username}': group '{sourceGroupName}' was not found.");

                        continue;
                    }

                    var modifyRequest = new ModifyRequest(groupDn);

                    var addMember =
                        new DirectoryAttributeModification
                        {
                            Name = "member",
                            Operation =
                                DirectoryAttributeOperation.Add
                        };

                    addMember.Add(userDn);

                    modifyRequest.Modifications.Add(addMember);

                    connection.SendRequest(modifyRequest);

                    result.GroupMembershipsAdded++;
                }
            }
            catch (DirectoryOperationException ex)
            {
                result.Errors.Add(
                    $"User '{item.Username}': {ex.Message}");
            }
            catch (Exception ex)
            {
                result.Errors.Add(
                    $"User '{item.Username}': {ex.Message}");
            }
        }

        return result;
    }

    private string? FindGroupDn(
        LdapConnection connection,
        string groupName)
    {
        var request = new SearchRequest(
            _options.GroupsDn,
            $"(cn={EscapeFilterValue(groupName)})",
            SearchScope.OneLevel,
            new[] { "cn" });

        var response =
            (SearchResponse)connection.SendRequest(request);

        return response.Entries.Count > 0
            ? response.Entries[0].DistinguishedName
            : null;
    }

    private static string NormalizeGroupName(string groupName)
    {
        return groupName.Equals(
            "administrators",
            StringComparison.OrdinalIgnoreCase)
            ? "Administrators"
            : groupName;
    }

    private static string EscapeDnValue(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace(",", "\\,")
            .Replace("+", "\\+")
            .Replace("\"", "\\\"")
            .Replace("<", "\\<")
            .Replace(">", "\\>")
            .Replace(";", "\\;")
            .Replace("=", "\\=");
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
