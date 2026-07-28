namespace BDIP.Contracts.Users.Requests;

public class CreateUserRequest
{
    public string Username { get; set; } = "";

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Unit { get; set; } = "";

    public string Password { get; set; } = "";

    public bool Enabled { get; set; } = true;
}