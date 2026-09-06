using BDIP.Contracts.Auth;

namespace BDIP.Application.Auth;

public interface IBdipSessionService
{
    string Create(LoginResponse user);

    bool TryRead(string? token, out LoginResponse user);
}
