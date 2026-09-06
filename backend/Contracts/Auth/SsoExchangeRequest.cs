namespace BDIP.Contracts.Auth;

public class SsoExchangeRequest
{
    public string Code { get; set; } = "";
    public string RedirectUri { get; set; } = "";
}
