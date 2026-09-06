using BDIP.Contracts.FingerMachineGlobalPolicy;

namespace BDIP.Application.FingerMachineGlobalPolicy;

public interface IFingerMachineGlobalPolicyService
{
    Task<FingerMachineGlobalPolicyResponse>
        GetAsync();

    Task<FingerMachineGlobalPolicyResponse>
        UpdateAsync(
            UpdateFingerMachineGlobalPolicyRequest request);
}
