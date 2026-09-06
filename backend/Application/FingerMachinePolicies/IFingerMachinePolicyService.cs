using BDIP.Contracts.FingerMachinePolicies;

namespace BDIP.Application.FingerMachinePolicies;

public interface IFingerMachinePolicyService
{
    Task<FingerMachinePolicyResponse?> GetByMachineCodeAsync(
        string machineCode);

    Task<FingerMachinePolicyResponse> UpsertAsync(
        string machineCode,
        UpdateFingerMachinePolicyRequest request);
}
