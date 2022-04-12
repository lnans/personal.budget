namespace Application.Common.Helpers;

public static class JwtHelper
{
    public static string CreateJwtToken(JwtSettings jwtSettings, User defaultUser)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, defaultUser.Username),
                new Claim(JwtRegisteredClaimNames.Jti, defaultUser.Id)
            }),
            Expires = DateTime.UtcNow.AddHours(6),
            Audience = jwtSettings.Audience,
            Issuer = jwtSettings.Issuer,
            SigningCredentials = credentials
        };
        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        return jwtToken;
    }
}