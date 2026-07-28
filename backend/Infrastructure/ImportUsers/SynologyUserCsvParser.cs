using System.DirectoryServices.Protocols;
using System.Text;

using BDIP.Application.ImportUsers;
using BDIP.Contracts.Users.Import;
using BDIP.Infrastructure.LDAP;

namespace BDIP.Infrastructure.ImportUsers;

public class SynologyUserCsvParser : ISynologyUserCsvParser
{
    private readonly ILdapConnectionFactory _ldap;
    private readonly LdapOptions _options;

    public SynologyUserCsvParser(
        ILdapConnectionFactory ldap,
        Microsoft.Extensions.Options.IOptions<LdapOptions> options)
    {
        _ldap = ldap;
        _options = options.Value;
    }

    public async Task<SynologyUserImportPreviewResponse> PreviewAsync(
        Stream stream)
    {
        await Task.CompletedTask;

        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);

        var lines = new List<string>();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();

            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        if (lines.Count == 0)
        {
            throw new InvalidOperationException(
                "CSV file is empty.");
        }

        var headers = ParseCsvLine(lines[0]);

        var nameIndex = GetColumnIndex(headers, "Name");
        var descriptionIndex = GetColumnIndex(headers, "Description");
        var emailIndex = GetColumnIndex(headers, "Email");
        var groupsIndex = GetColumnIndex(headers, "User groups");
        var statusIndex = GetColumnIndex(headers, "Status");

        if (nameIndex < 0)
        {
            throw new InvalidOperationException(
                "CSV column 'Name' was not found.");
        }

        var preview = new SynologyUserImportPreviewResponse
        {
            TotalRows = lines.Count - 1
        };

        var items = new List<SynologyUserImportPreviewItem>();

        for (var index = 1; index < lines.Count; index++)
        {
            var columns = ParseCsvLine(lines[index]);

            var sourceUsername = GetValue(columns, nameIndex).Trim();

            if (string.IsNullOrWhiteSpace(sourceUsername))
            {
                continue;
            }

            var username = sourceUsername.ToLowerInvariant();

            var email = GetValue(columns, emailIndex).Trim();
            var status = GetValue(columns, statusIndex).Trim();

            var groups = SplitGroups(
                GetValue(columns, groupsIndex));

            var fullName = ToDisplayName(sourceUsername);

            var enabled = !IsDisabled(status);

            items.Add(new SynologyUserImportPreviewItem
            {
                RowNumber = index + 1,
                SourceUsername = sourceUsername,
                Username = username,
                FullName = fullName,
                Email = email,
                Status = status,
                Enabled = enabled,
                Groups = groups
            });
        }

        var duplicateUsernames = items
            .GroupBy(item => item.Username)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var ldapUsernames = GetExistingLdapUsernames();

        foreach (var item in items)
        {
            item.IsDuplicateInCsv =
                duplicateUsernames.Contains(item.Username);

            item.ExistsInLdap =
                ldapUsernames.Contains(item.Username);

            if (item.IsDuplicateInCsv)
            {
                item.Action = "Skip";
                item.Note =
                    "Duplicate username after lowercase normalization.";
            }
            else if (item.ExistsInLdap)
            {
                item.Action = "Skip";
                item.Note = "User already exists in LDAP.";
            }
            else
            {
                item.Action = "Create";
                item.Note = item.Enabled
                    ? "New active user."
                    : "New disabled user.";
            }
        }

        preview.Users = items
            .OrderBy(item => item.Username)
            .ToList();

        preview.ValidRows = preview.Users.Count;

        preview.NewUsers = preview.Users.Count(
            item => item.Action == "Create");

        preview.ExistingUsers = preview.Users.Count(
            item => item.ExistsInLdap);

        preview.DuplicateUsernames = preview.Users.Count(
            item => item.IsDuplicateInCsv);

        preview.UsersWithoutEmail = preview.Users.Count(
            item => string.IsNullOrWhiteSpace(item.Email));

        preview.DisabledUsers = preview.Users.Count(
            item => !item.Enabled);

        preview.GroupsFound = preview.Users
            .SelectMany(item => item.Groups)
            .Where(group => !string.IsNullOrWhiteSpace(group))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group)
            .ToList();

        return preview;
    }

    private HashSet<string> GetExistingLdapUsernames()
    {
        using var connection = _ldap.Create();

        var request = new SearchRequest(
            _options.PeopleDn,
            "(objectClass=inetOrgPerson)",
            SearchScope.OneLevel,
            new[] { "uid" });

        var response =
            (SearchResponse)connection.SendRequest(request);

        var usernames = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (SearchResultEntry entry in response.Entries)
        {
            var username =
                entry.Attributes["uid"]?[0]?.ToString();

            if (!string.IsNullOrWhiteSpace(username))
            {
                usernames.Add(username);
            }
        }

        return usernames;
    }

    private static bool IsDisabled(string status)
    {
        return status.Contains(
            "disabled",
            StringComparison.OrdinalIgnoreCase)
            || status.Contains(
                "disable",
                StringComparison.OrdinalIgnoreCase)
            || status.Contains(
                "deactivated",
                StringComparison.OrdinalIgnoreCase)
            || status.Contains(
                "inactive",
                StringComparison.OrdinalIgnoreCase);
    }

    private static int GetColumnIndex(
        IReadOnlyList<string> headers,
        string columnName)
    {
        for (var index = 0; index < headers.Count; index++)
        {
            if (string.Equals(
                headers[index].Trim(),
                columnName,
                StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    private static string GetValue(
        IReadOnlyList<string> columns,
        int index)
    {
        if (index < 0 || index >= columns.Count)
        {
            return "";
        }

        return columns[index];
    }

    private static string ToDisplayName(string username)
    {
        var words = username
            .Split(
                new[] { '.', '_', '-', ' ' },
                StringSplitOptions.RemoveEmptyEntries)
            .Select(word =>
                char.ToUpperInvariant(word[0]) +
                word[1..].ToLowerInvariant());

        return string.Join(" ", words);
    }

    private static List<string> SplitGroups(string value)
    {
        return value
            .Split(
                new[] { ';', ',', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries)
            .Select(group => group.Trim())
            .Where(group => !string.IsNullOrWhiteSpace(group))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var value = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];

            if (character == '"')
            {
                if (
                    inQuotes &&
                    index + 1 < line.Length &&
                    line[index + 1] == '"')
                {
                    value.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (character == ',' && !inQuotes)
            {
                result.Add(value.ToString());
                value.Clear();
                continue;
            }

            value.Append(character);
        }

        result.Add(value.ToString());

        return result;
    }
}
