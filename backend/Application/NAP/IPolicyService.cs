using BDIP.Domain.NAP;

namespace BDIP.Application.NAP;

public interface IPolicyService
{
    Task<IEnumerable<Policy>> GetAllAsync();

    Task<Policy?> GetByIdAsync(Guid id);

    Task<Policy?> GetByCodeAsync(string code);

    Task<Policy> CreateAsync(Policy policy);

    Task<Policy> UpdateAsync(Policy policy);

    Task DeleteAsync(Guid id);
}