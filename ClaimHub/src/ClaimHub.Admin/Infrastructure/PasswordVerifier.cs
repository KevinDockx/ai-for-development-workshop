using System.Security.Cryptography;
using System.Text;

namespace ClaimHub.Admin.Infrastructure;

public static class PasswordVerifier
{
    public static bool Verify(string password, string storedHash)
    {
        string hash = Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(password)));
        return string.Equals(hash, storedHash, StringComparison.Ordinal);
    }
}
