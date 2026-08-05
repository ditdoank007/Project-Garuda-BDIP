using BDIP.Contracts.Units;

namespace BDIP.Application.Units;

public interface IUnitService
{
    Task<List<UnitResponse>> GetAllAsync();

    Task<UnitResponse?> GetByNameAsync(string name);

    Task<UnitResponse> CreateAsync(CreateUnitRequest request);

    Task<UnitResponse> UpdateAsync(
        string currentName,
        UpdateUnitRequest request);

    Task DeleteAsync(string name);
}
