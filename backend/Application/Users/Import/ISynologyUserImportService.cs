using BDIP.Contracts.Users.Import;

namespace BDIP.Application.Users.Import;

public interface ISynologyUserImportService
{
    Task<ExecuteSynologyUserImportResponse> ExecuteAsync(
        ExecuteSynologyUserImportRequest request
    );
}
