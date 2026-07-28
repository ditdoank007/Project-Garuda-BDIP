using BDIP.Contracts.Users;
using BDIP.Contracts.Users.Requests;

namespace BDIP.Application.Users;

public interface IUserService
{
    Task<UserListResponse> GetUsersAsync();

    Task CreateUserAsync(
        CreateUserRequest request
    );

    Task UpdateUserAsync(
        string username,
        UpdateUserRequest request
    );

    Task ResetPasswordAsync(
        string username,
        ResetUserPasswordRequest request
    );

    Task UpdateUserStatusAsync(
        string username,
        UpdateUserStatusRequest request
    );

    Task DeleteUserAsync(
        string username
    );
}
