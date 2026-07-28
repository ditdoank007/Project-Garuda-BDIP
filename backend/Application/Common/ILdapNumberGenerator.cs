namespace BDIP.Application.Common;

public interface ILdapNumberGenerator
{
    Task<int> GenerateUidNumberAsync();

    Task<int> GenerateGidNumberAsync();
}