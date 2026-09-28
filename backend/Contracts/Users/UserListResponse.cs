namespace BDIP.Contracts.Users;

public class UserListSummary
{
    public int TotalUsers { get; set; }

    public int TotalEmails { get; set; }

    public int ChangedPasswords { get; set; }

    public int DefaultPasswords { get; set; }
}

public class UserListResponse
{
    public List<UserResponse> Users { get; set; } = new();

    public UserListSummary Summary { get; set; } = new();

    public int Total => Users.Count;
}
