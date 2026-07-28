using System.Text;
using BDIP.Application.ImportGroups;
using BDIP.Contracts.ImportGroups;

namespace BDIP.Infrastructure.ImportGroups;

public sealed class CsvGroupParser : ICsvGroupParser
{
    public async Task<IReadOnlyList<CsvGroupRecord>> ParseAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default)
    {
        var records = new List<CsvGroupRecord>();

        using var reader = new StreamReader(
            csvStream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        var header = await reader.ReadLineAsync();

        if (header == null)
            return records;

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var columns = ParseCsvLine(line);

            if (columns.Count == 0)
                continue;

            var record = new CsvGroupRecord
            {
                GroupName = GetColumn(columns, 0),
                Description = GetColumn(columns, 1),
                Members = ParseMembers(GetColumn(columns, 2))
            };

            records.Add(record);
        }

        return records;
    }

    private static string GetColumn(
        IReadOnlyList<string> columns,
        int index)
    {
        if (index >= columns.Count)
            return "";

        return columns[index].Trim();
    }

    private static List<string> ParseMembers(
        string members)
    {
        if (string.IsNullOrWhiteSpace(members))
            return new();

        return members
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToList();
    }

    private static List<string> ParseCsvLine(
        string line)
    {
        var result = new List<string>();

        var current = new StringBuilder();

        var inQuotes = false;

        foreach (var c in line)
        {
            switch (c)
            {
                case '"':
                    inQuotes = !inQuotes;
                    break;

                case ',' when !inQuotes:
                    result.Add(current.ToString());
                    current.Clear();
                    break;

                default:
                    current.Append(c);
                    break;
            }
        }

        result.Add(current.ToString());

        return result;
    }
}