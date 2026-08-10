using BDIP.Contracts.Applications;

namespace BDIP.Application.Applications;

public interface IApplicationService
{
    Task<List<ApplicationResponse>> GetAllAsync();

    Task<ApplicationResponse?> GetByCodeAsync(
        string code);

    Task<ApplicationResponse> CreateAsync(
        CreateApplicationRequest request);

    Task<ApplicationResponse> UpdateAsync(
        string currentCode,
        UpdateApplicationRequest request);

    Task DeactivateAsync(
        string code);
}
