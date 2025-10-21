using Application.Interfaces;
using Domain.Users;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<RefreshTokenResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthTokenGenerator _authTokenGenerator;

    public RefreshTokenHandler(IAppDbContext dbContext, IAuthTokenGenerator authTokenGenerator)
    {
        _dbContext = dbContext;
        _authTokenGenerator = authTokenGenerator;
    }

    public async Task<ErrorOr<RefreshTokenResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        var userId = _authTokenGenerator.ValidateRefreshToken(command.RefreshToken);
        if (userId is null)
        {
            return UserErrors.UserInvalidRefreshToken;
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return UserErrors.UserInvalidRefreshToken;
        }

        var bearer = user.GenerateAuthToken(_authTokenGenerator);
        var refreshToken = user.GenerateRefreshToken(_authTokenGenerator);

        return new RefreshTokenResponse(bearer, refreshToken);
    }
}
