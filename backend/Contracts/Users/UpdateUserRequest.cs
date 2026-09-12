namespace BDIP.Contracts.Users.Requests;

public class UpdateUserRequest
{
    public string Username { get; set; } = "";

    public string Nip { get; set; } = "";

    public string FingerId { get; set; } = "";

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Unit { get; set; } = "";

    public bool Enabled { get; set; } = true;
}
