using System.Security;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Personal.Budget.Api.Common;
using Personal.Budget.Api.Helpers;
using Personal.Budget.Api.Persistence;

namespace Personal.Budget.Api.Features.Authentication.SignIn;

public class SignInEndpoint : Endpoint<SignInRequest, SignInResponse>
{
    private readonly ApiDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public SignInEndpoint(ApiDbContext dbContext, JwtSettings jwtSettings)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings;
    }

    public override void Configure()
    {
        Post("auth");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SignInRequest req, CancellationToken ct)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username.Equals(req.Username), ct);
        
        if (user is null || user.Hash != HashHelper.GenerateHash(user.Username, req.Password)) ThrowError("authentication failed");

        var jwtToken = JWTBearer.CreateToken(
            signingKey: _jwtSettings.Key,
            audience: _jwtSettings.Audience,
            issuer: _jwtSettings.Issuer,
            expireAt: DateTime.UtcNow.AddDays(1),
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString())
            });

        await SendOkAsync(new SignInResponse {Token = jwtToken}, ct);
    }
}