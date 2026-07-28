namespace BDIP.Contracts.Users;

public class UserListResponse
{
    public List<UserResponse> Users { get; set; } = new();

    public int Total => Users.Count;
}