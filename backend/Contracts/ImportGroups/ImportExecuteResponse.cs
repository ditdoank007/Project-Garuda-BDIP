namespace BDIP.Contracts.ImportGroups;

public sealed class ImportExecuteResponse
{
    public int Imported { get; set; }

    public int Skipped { get; set; }

    public int Failed { get; set; }

    public List<ImportResultItem> Details { get; set; }
        = new();
}