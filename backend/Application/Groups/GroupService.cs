using BDIP.Application.Common;
using BDIP.Contracts.Groups;
using BDIP.Domain.Entities;

namespace BDIP.Application.Groups;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILdapNumberGenerator _numberGenerator;
    private readonly IGroupMemberReader _memberReader;
    private readonly IUserDnResolver _userDnResolver;

    public GroupService(
        IGroupRepository groupRepository,
        ILdapNumberGenerator numberGenerator,
        IGroupMemberReader memberReader,
        IUserDnResolver userDnResolver)
    {
        _groupRepository = groupRepository;
        _numberGenerator = numberGenerator;
        _memberReader = memberReader;
        _userDnResolver = userDnResolver;
    }

    public async Task<GroupResponse> CreateAsync(
        CreateGroupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Group name is required.");

        var exists = await _groupRepository.ExistsAsync(
            request.Name);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Group '{request.Name}' already exists.");
        }

        var group = new Group
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            GidNumber =
                await _numberGenerator.GenerateGidNumberAsync()
        };

        var created = await _groupRepository.CreateAsync(group);

        return ToResponse(created);
    }

    public async Task<GroupResponse?> GetByNameAsync(
        string groupName)
    {
        var group = await _groupRepository.GetByNameAsync(
            groupName);

        return group == null
            ? null
            : ToResponse(group);
    }

    public async Task<IReadOnlyList<GroupResponse>> GetAllAsync()
    {
        var groups = await _groupRepository.GetAllAsync();

        return groups
            .Select(ToResponse)
            .ToList();
    }

    public async Task<GroupMembersResponse?> GetMembersAsync(
        string groupName)
    {
        return await _memberReader.GetMembersAsync(groupName);
    }

    public async Task<bool> ExistsAsync(string groupName)
    {
        return await _groupRepository.ExistsAsync(groupName);
    }

    public async Task DeleteAsync(string groupName)
    {
        await _groupRepository.DeleteAsync(groupName);
    }

    public async Task<GroupResponse> UpdateAsync(
        string groupName,
        CreateGroupRequest request)
    {
        var group = await _groupRepository.GetByNameAsync(
            groupName);

        if (group == null)
        {
            throw new InvalidOperationException(
                $"Group '{groupName}' not found.");
        }

        group.Description = request.Description?.Trim();

        var updated = await _groupRepository.UpdateAsync(group);

        return ToResponse(updated);
    }

    public async Task AddMemberAsync(
        string groupName,
        string username)
    {
        var userDn = await _userDnResolver.GetUserDnAsync(username);

        if (string.IsNullOrWhiteSpace(userDn))
        {
            throw new InvalidOperationException(
                $"User '{username}' not found.");
        }

        await _groupRepository.AddMemberAsync(
            groupName,
            userDn);
    }

    public async Task RemoveMemberAsync(
        string groupName,
        string username)
    {
        var userDn = await _userDnResolver.GetUserDnAsync(username);

        if (string.IsNullOrWhiteSpace(userDn))
        {
            throw new InvalidOperationException(
                $"User '{username}' not found.");
        }

        await _groupRepository.RemoveMemberAsync(
            groupName,
            userDn);
    }

    private static GroupResponse ToResponse(Group group)
    {
        return new GroupResponse
        {
            Name = group.Name,
            Description = group.Description,
            DistinguishedName = group.DistinguishedName,
            GidNumber = group.GidNumber,
            MemberCount = group.Members.Count,
            CreatedAt = group.CreatedAt
        };
    }
}
