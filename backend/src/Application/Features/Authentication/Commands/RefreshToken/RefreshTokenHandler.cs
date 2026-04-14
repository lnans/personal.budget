using Application.Extensions;
using Application.Interfaces;
using Domain.Users;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
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
    ) =>
        await _authTokenGenerator
            .ValidateRefreshToken(command.RefreshToken)
            .ThenAsync(userId => GetUserByIdAsync(userId, cancellationToken))
            .MatchFirst(
                user =>
                    new RefreshTokenResponse(
                        user.GenerateAuthToken(_authTokenGenerator),
                        user.GenerateRefreshToken(_authTokenGenerator)
                    ).ToErrorOr(),
                error => error
            );

    private async Task<ErrorOr<User>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        await _dbContext
            .Users.AsNoTracking()
            .FirstOrErrorAsync(user => user.Id == userId, UserErrors.UserInvalidToken, cancellationToken);
}
