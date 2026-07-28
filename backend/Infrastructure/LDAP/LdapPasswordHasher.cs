using System.Security.Cryptography;
using System.Text;

namespace BDIP.Infrastructure.LDAP;

public static class LdapPasswordHasher
{
    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(4);

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        byte[] passwordWithSalt = new byte[
            passwordBytes.Length + salt.Length];

        Buffer.BlockCopy(
            passwordBytes,
            0,
            passwordWithSalt,
            0,
            passwordBytes.Length);

        Buffer.BlockCopy(
            salt,
            0,
            passwordWithSalt,
            passwordBytes.Length,
            salt.Length);

        byte[] hash =
            SHA1.HashData(passwordWithSalt);

        byte[] hashWithSalt = new byte[
            hash.Length + salt.Length];

        Buffer.BlockCopy(
            hash,
            0,
            hashWithSalt,
            0,
            hash.Length);

        Buffer.BlockCopy(
            salt,
            0,
            hashWithSalt,
            hash.Length,
            salt.Length);

        return "{SSHA}" +
            Convert.ToBase64String(hashWithSalt);
    }
}