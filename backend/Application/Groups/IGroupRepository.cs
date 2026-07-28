using BDIP.Domain.Entities;

namespace BDIP.Application.Groups;

public interface IGroupRepository
{
    #region Query

    Task<bool> ExistsAsync(
        string groupName,
        CancellationToken cancellationToken = default);

    Task<Group?> GetByNameAsync(
        string groupName);

    Task<IReadOnlyList<Group>> GetAllAsync();

    #endregion

    #region Command

    Task<Group> CreateAsync(
        Group group,
        CancellationToken cancellationToken = default);

    Task<Group> UpdateAsync(
        Group group,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string groupName,
        CancellationToken cancellationToken = default);

    Task AddMemberAsync(
        string groupName,
        string memberDn,
        CancellationToken cancellationToken = default);

    Task RemoveMemberAsync(
        string groupName,
        string memberDn,
        CancellationToken cancellationToken = default);

    #endregion
}