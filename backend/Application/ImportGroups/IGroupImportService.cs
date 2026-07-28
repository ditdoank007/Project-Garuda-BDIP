using BDIP.Contracts.ImportGroups;

namespace BDIP.Application.ImportGroups;

public interface IGroupImportService
{
    Task<ImportPreviewResponse> PreviewAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default);

    Task<ImportExecuteResponse> ImportAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default);
}