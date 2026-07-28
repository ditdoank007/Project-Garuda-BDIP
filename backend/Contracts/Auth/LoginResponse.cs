namespace BDIP.Contracts.Auth;

public class LoginResponse
{
    public string Username { get; set; } = "";

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Role { get; set; } = "User";
}
