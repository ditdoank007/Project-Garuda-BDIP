namespace BDIP.Contracts.Users;

public class UserResponse
{
    public string Uid { get; set; } = "";

    public string Username { get; set; } = "";

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Unit { get; set; } = "";

    public bool Enabled { get; set; }
}

public class UsersResponse
{
    public List<UserResponse> Users { get; set; } = new();

    public int Total => Users.Count;
}