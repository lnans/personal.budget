using System.Security.Authentication;
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Authentication.Commands.SignIn;

public record SignInRequest : IRequest<AuthenticationDto>
{
    public string Username { get; init; }
    public string Password { get; init; }
}

public class SignInCommandHandler : IRequestHandler<SignInRequest, AuthenticationDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public SignInCommandHandler(IApplicationDbContext dbContext, JwtSettings jwtSettings)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthenticationDto> Handle(SignInRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username.Equals(request.Username), cancellationToken);
        if (user is null || user.Hash != HashHelper.GenerateHash(user.Id, request.Password)) throw new AuthenticationException();

        var jwtToken = JwtHelper.CreateJwtToken(_jwtSettings, user);

        return new AuthenticationDto {Username = user.Username, Token = jwtToken};
    }
}