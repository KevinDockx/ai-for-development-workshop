using System.Security.Cryptography;
using System.Text;

namespace ClaimHub.Api.Security;

public static class PasswordHasher
{
    public static string Hash(string password) =>
        Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(password)));

    public static bool Verify(string password, string storedHash) =>
        string.Equals(Hash(password), storedHash, StringComparison.Ordinal);
}
