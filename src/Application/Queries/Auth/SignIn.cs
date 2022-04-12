using System.Security.Authentication;
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Auth;

public record SignInRequest(string Username, string Password) : IRequest<SignInResponse>;
public record SignInResponse(string Username, string Token);

public class SignIn : IRequestHandler<SignInRequest, SignInResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public SignIn(IApplicationDbContext dbContext, JwtSettings jwtSettings)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings;
    }

    public async Task<SignInResponse> Handle(SignInRequest request, CancellationToken cancellationToken)
    {
        var (username, password) = request;
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username.Equals(username), cancellationToken);
        if (user is null || user.Hash != HashHelper.GenerateHash(user.Id, password)) throw new AuthenticationException();

        var jwtToken = JwtHelper.CreateJwtToken(_jwtSettings, user);

        return new SignInResponse(user.Username, jwtToken);
    }
}