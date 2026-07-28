namespace BDIP.Contracts.ImportGroups;

public sealed class ImportPreviewItem
{
    public string GroupName { get; set; } = "";

    public string Description { get; set; } = "";

    public int MemberCount { get; set; }

    public bool Exists { get; set; }

    public string Status { get; set; } = "";
}