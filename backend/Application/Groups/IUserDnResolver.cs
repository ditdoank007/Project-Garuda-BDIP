namespace BDIP.Application.Groups;

public interface IUserDnResolver
{
    Task<string?> GetUserDnAsync(string username);
}
