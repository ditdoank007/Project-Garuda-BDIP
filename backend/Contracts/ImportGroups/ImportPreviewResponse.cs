namespace BDIP.Contracts.ImportGroups;

public sealed class ImportPreviewResponse
{
    public List<ImportPreviewItem> Groups { get; set; }
        = new();

    public int TotalRows { get; set; }

    public int NewGroups { get; set; }

    public int ExistingGroups { get; set; }

    public int InvalidRows { get; set; }
}