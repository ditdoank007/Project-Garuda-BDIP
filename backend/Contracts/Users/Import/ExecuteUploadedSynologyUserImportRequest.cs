namespace BDIP.Contracts.Users.Import;

public class ExecuteUploadedSynologyUserImportRequest
{
    public string UploadId { get; set; } = "";

    public string InitialPassword { get; set; } = "";
}
