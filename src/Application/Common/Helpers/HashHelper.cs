using System.Security.Cryptography;
using System.Text;

namespace Application.Common.Helpers;

public static class HashHelper
{
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