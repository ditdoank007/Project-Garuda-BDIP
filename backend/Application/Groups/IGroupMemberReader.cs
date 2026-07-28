using BDIP.Contracts.Groups;

namespace BDIP.Application.Groups;

public interface IGroupMemberReader
{
    Task<GroupMembersResponse?> GetMembersAsync(
        string groupName);
}
