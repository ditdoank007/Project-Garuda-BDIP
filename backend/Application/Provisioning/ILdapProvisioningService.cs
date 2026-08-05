using BDIP.Contracts.Users.Requests;

namespace BDIP.Application.Provisioning;

public interface ILdapProvisioningService
{
    Task CreateUserAsync(CreateUserRequest request);

    Task UpdateUserAsync(
        string username,
        UpdateUserRequest request);

    Task DeleteUserAsync(
        string username);

    Task UpdateUserStatusAsync(
        string username,
        UpdateUserStatusRequest request);

    Task ResetPasswordAsync(
        string username,
        ResetUserPasswordRequest request);
}