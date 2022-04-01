using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Application.Commands.Auth;

public record SignInCommand(string Username, string Password) : IRequest<SignInResponse>;
public record SignInResponse(string Username, string Token);

public class SignIn : IRequestHandler<SignInCommand, SignInResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public SignIn(IApplicationDbContext dbContext, JwtSettings jwtSettings)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings;
    }

    public async Task<SignInResponse> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var (username, password) = request;
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username.Equals(username), cancellationToken);
        if (user is null || user.Hash != Utils.GenerateHash(user.Id, password)) throw new AuthenticationException();

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, user.Id)
            }),
            Expires = DateTime.UtcNow.AddHours(6),
            Audience = _jwtSettings.Audience,
            Issuer = _jwtSettings.Issuer,
            SigningCredentials = credentials
        };
        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        return new SignInResponse(user.Username, jwtToken);
    }
}