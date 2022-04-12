namespace Application;

public static class Utils
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

    public static string ToMidnightIsoString(this DateTime date)
    {
        var midnight = new DateTime(date.Year, date.Month, date.Day);
        return midnight.ToString("o");
    }
}