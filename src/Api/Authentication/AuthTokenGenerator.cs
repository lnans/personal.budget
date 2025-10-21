using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Users;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Api.Authentication;

internal class AuthTokenGenerator : IAuthTokenGenerator
{
    private readonly AuthTokenOptions _options;
    private readonly TimeProvider _timeProvider;

    public AuthTokenGenerator(AuthTokenOptions options, TimeProvider timeProvider)
    {
        _options = options;
        _timeProvider = timeProvider;
    }

    public string GenerateToken(Guid userId, string userLogin)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, userLogin),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                _timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64
            ),
        };

        return CreateToken(claims, _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(_options.ExpirationMinutes));
    }

    public string GenerateRefreshToken(Guid userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("token_type", "refresh"),
        };

        return CreateToken(claims, _timeProvider.GetUtcNow().UtcDateTime.AddDays(_options.RefreshTokenExpirationDays));
    }

    private string CreateToken(Claim[] claims, DateTime expires)
    {
        var credentials = CreateSigningCredentials();

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: _timeProvider.GetUtcNow().UtcDateTime,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private SigningCredentials CreateSigningCredentials()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public Guid? ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);

            var tokenTypeClaim = principal.Claims.FirstOrDefault(c => c.Type == "token_type");
            if (tokenTypeClaim?.Value != "refresh")
            {
                return null;
            }

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
