using BDIP.Domain.NAP;

namespace BDIP.Application.NAP;

public interface IUserNapService
{
    Task<UserNap?> GetByUidAsync(string uid);

    Task<IReadOnlyList<UserNap>> GetAllAsync();

    Task<UserNap> UpdatePolicyAsync(
        string uid,
        Guid? policyId,
        string? policyCode);
}
