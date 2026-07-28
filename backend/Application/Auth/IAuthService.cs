using BDIP.Contracts.Auth;

namespace BDIP.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
