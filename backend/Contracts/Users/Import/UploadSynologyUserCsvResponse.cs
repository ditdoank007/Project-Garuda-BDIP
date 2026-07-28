namespace BDIP.Contracts.Users.Import;

public class UploadSynologyUserCsvResponse
{
    public string UploadId { get; set; } = "";

    public string FileName { get; set; } = "";

    public long FileSize { get; set; }
}
