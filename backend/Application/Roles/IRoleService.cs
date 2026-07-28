using BDIP.Contracts.Roles;

namespace BDIP.Application.Roles;

public interface IRoleService
{
    Task<IReadOnlyList<RoleResponse>> GetAllAsync();

    Task<RoleResponse?> GetByNameAsync(
        string name);

    Task<RoleMembersResponse?> GetMembersAsync(
        string name);

    Task<RoleResponse> CreateAsync(
        CreateRoleRequest request);

    Task<RoleResponse?> UpdateAsync(
        string name,
        UpdateRoleRequest request);

    Task<bool> DeleteAsync(
        string name);

    Task AddMemberAsync(
        string name,
        string username);

    Task RemoveMemberAsync(
        string name,
        string username);
}
