using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Api.IntegrationTests;

public static class FakeJwtManager
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private static readonly RandomNumberGenerator Generator = RandomNumberGenerator.Create();
    private static readonly byte[] Key = new byte[32];

    static FakeJwtManager()
    {
        Generator.GetBytes(Key);
        SecurityKey = new SymmetricSecurityKey(Key) { KeyId = Guid.NewGuid().ToString() };
        SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
    }

    public static string Issuer { get; } = Guid.NewGuid().ToString();
    public static string Audience { get; } = Guid.NewGuid().ToString();
    public static string UserId { get; } = Guid.NewGuid().ToString();
    public static SecurityKey SecurityKey { get; }
    private static SigningCredentials SigningCredentials { get; }

    public static string GenerateJwtToken() =>
        TokenHandler.WriteToken(
            new JwtSecurityToken(
                Issuer,
                Audience,
                new[] { new Claim(ClaimTypes.NameIdentifier, UserId) },
                null,
                DateTime.UtcNow.AddMinutes(10), SigningCredentials)
        );
}