using BDIP.Contracts.Locations;

namespace BDIP.Application.Locations;

public interface ILocationService
{
    Task<List<LocationResponse>> GetAllAsync();

    Task<LocationResponse?> GetByNameAsync(
        string name);

    Task<LocationResponse> CreateAsync(
        CreateLocationRequest request);

    Task<LocationResponse> UpdateAsync(
        string currentName,
        UpdateLocationRequest request);

    Task DeleteAsync(
        string name);
}
