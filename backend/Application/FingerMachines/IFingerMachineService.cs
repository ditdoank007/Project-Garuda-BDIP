using BDIP.Contracts.FingerMachines;

namespace BDIP.Application.FingerMachines;

public interface IFingerMachineService
{
    Task<List<FingerMachineResponse>> GetAllAsync();

    Task<FingerMachineResponse?> GetByCodeAsync(string code);

    Task<FingerMachineResponse> CreateAsync(
        CreateFingerMachineRequest request);

    Task<FingerMachineResponse> UpdateAsync(
        string code,
        UpdateFingerMachineRequest request);

    Task DeactivateAsync(string code);

    Task DeleteAsync(string code);
}
