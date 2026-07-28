namespace BDIP.Contracts.ImportGroups;

public sealed class ImportResultItem
{
    public string GroupName { get; set; } = "";

    public string Status { get; set; } = "";

    public string? Message { get; set; }
}