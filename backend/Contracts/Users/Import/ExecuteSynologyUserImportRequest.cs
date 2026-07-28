namespace BDIP.Contracts.Users.Import;

public class ExecuteSynologyUserImportRequest
{
    public string CsvPath { get; set; } = "";

    public string InitialPassword { get; set; } = "";
}
