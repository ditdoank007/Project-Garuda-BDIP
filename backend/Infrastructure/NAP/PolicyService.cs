using BDIP.Application.NAP;
using BDIP.Domain.NAP;

namespace BDIP.Infrastructure.NAP;

public sealed class PolicyService : IPolicyService
{
    private readonly List<Policy> _policies = new();

    public Task<IEnumerable<Policy>> GetAllAsync()
    {
        Console.WriteLine(
            $"GET Instance={GetHashCode()} Count={_policies.Count}");
        return Task.FromResult(_policies.AsEnumerable());
    }

    public Task<Policy?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(
            _policies.FirstOrDefault(p => p.Id == id));
    }

    public Task<Policy?> GetByCodeAsync(string code)
    {
        return Task.FromResult(
            _policies.FirstOrDefault(p =>
                p.Code.Equals(code, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<Policy> CreateAsync(Policy policy)
    {
        _policies.Add(policy);
        return Task.FromResult(policy);
    }

    public Task<Policy> UpdateAsync(Policy policy)
    {
        var existing = _policies.FirstOrDefault(p => p.Id == policy.Id);

        if (existing is null)
            throw new KeyNotFoundException("Policy not found.");

        existing.Code = policy.Code;
        existing.Name = policy.Name;
        existing.Description = policy.Description;
        existing.IsActive = policy.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult(existing);
    }

    public Task DeleteAsync(Guid id)
    {
        var existing = _policies.FirstOrDefault(p => p.Id == id);

        if (existing is not null)
            _policies.Remove(existing);

        return Task.CompletedTask;
    }

    public PolicyService()
    {
        Console.WriteLine(
            $"PolicyService Instance: {GetHashCode()}");
    }
}