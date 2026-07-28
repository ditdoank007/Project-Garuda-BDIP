using BDIP.Contracts.Groups;

namespace BDIP.Application.Groups;

public interface IGroupService
{
    Task<GroupResponse> CreateAsync(
        CreateGroupRequest request);

    Task<GroupResponse?> GetByNameAsync(
        string groupName);

    Task<IReadOnlyList<GroupResponse>> GetAllAsync();

    Task<GroupMembersResponse?> GetMembersAsync(
        string groupName);

    Task<bool> ExistsAsync(string groupName);

    Task DeleteAsync(string groupName);

    Task<GroupResponse> UpdateAsync(
        string groupName,
        CreateGroupRequest request);

    Task AddMemberAsync(
        string groupName,
        string userDn);

    Task RemoveMemberAsync(
        string groupName,
        string userDn);
}
