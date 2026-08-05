using BDIP.Contracts.Users.Requests;
using BDIP.Domain.NAP;

namespace BDIP.Application.Provisioning;

public interface IRadiusProvisioningService
{
    Task CreateUserAsync(
        CreateUserRequest request);

    Task SyncPolicyAsync(
        Policy policy);

    Task DeletePolicyAsync(
        string policyCode);

    Task AssignUserGroupAsync(
        string username,
        string policyCode);

    Task RemoveUserGroupAsync(
        string username);

    Task DeleteUserAsync(
    string username);
}