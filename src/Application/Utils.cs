using System.Security.Cryptography;
using System.Text;

namespace Application;

public static class Utils
{
    public static Guid ToGuid(this string value)
    {
        var isValid = Guid.TryParse(value, out var guid);
        return isValid ? guid : Guid.Empty;
    }

    public static string GenerateHash(string salt, string input)
    {
        var sha512 = SHA512.Create();
        var bytes = Encoding.UTF8.GetBytes(salt + input);
        var hash = sha512.ComputeHash(bytes);
        var result = new StringBuilder();
        foreach (var hashByte in hash) result.Append(hashByte.ToString("X2"));
        return result.ToString();
    }
}