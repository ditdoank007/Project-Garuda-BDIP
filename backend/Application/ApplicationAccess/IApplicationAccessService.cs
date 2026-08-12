using BDIP.Contracts.ApplicationAccess;

namespace BDIP.Application.ApplicationAccess;

public interface IApplicationAccessService
{
    Task<List<ApplicationAccessResponse>> GetAllAsync();

    Task<ApplicationAccessResponse?> GetByIdAsync(
        Guid id);

    Task<ApplicationAccessResponse> CreateAsync(
        CreateApplicationAccessRequest request);

    Task<ApplicationAccessResponse> UpdateAsync(
        Guid id,
        UpdateApplicationAccessRequest request);

    Task DeleteAsync(
        Guid id);
}
