using BDIP.Contracts.Auth;

namespace BDIP.Application.Auth;

public interface ISsoAuthorizationCodeService
{
    string Create(LoginResponse user, string redirectUri);

    bool TryConsume(
        string code,
        string redirectUri,
        out LoginResponse user);
}
