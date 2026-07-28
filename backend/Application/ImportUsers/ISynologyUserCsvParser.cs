using BDIP.Contracts.Users.Import;

namespace BDIP.Application.ImportUsers;

public interface ISynologyUserCsvParser
{
    Task<SynologyUserImportPreviewResponse> PreviewAsync(
        Stream stream);
}
