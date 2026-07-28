namespace BDIP.Contracts.ImportGroups;

public sealed class CsvGroupRecord
{
    public string GroupName { get; set; } = "";

    public string Description { get; set; } = "";

    public List<string> Members { get; set; }
        = new();
}