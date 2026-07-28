namespace BDIP.Contracts.Users.Requests;

public class UpdateUserRequest
{
    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Unit { get; set; } = "";

    public bool Enabled { get; set; } = true;
}
