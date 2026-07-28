using BDIP.Contracts.Users.Import;

namespace BDIP.Application.Users.Import;

public interface ISynologyUserUploadService
{
    Task<UploadSynologyUserCsvResponse> UploadAsync(
        Stream stream,
        string fileName);

    Task<SynologyUserImportPreviewResponse> PreviewAsync(
        string uploadId);

    Task<ExecuteSynologyUserImportResponse> ExecuteAsync(
        ExecuteUploadedSynologyUserImportRequest request);
}
