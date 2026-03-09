using Application.Extensions;
using Application.Interfaces;
using Domain.Users;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Authentication.Commands.SignIn;

public sealed class SignInHandler : ICommandHandler<SignInCommand, SignInResponse>
{
    private readonly IAppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenGenerator _authTokenGenerator;

    public SignInHandler(
        IAppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IAuthTokenGenerator authTokenGenerator
    )
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _authTokenGenerator = authTokenGenerator;
    }

    public async Task<ErrorOr<SignInResponse>> Handle(SignInCommand command, CancellationToken cancellationToken) =>
        await GetUserByLoginAsync(command.Login, cancellationToken)
            .Then(user => user.VerifyPassword(command.Password, _passwordHasher))
            .MatchFirst(
                user =>
                    new SignInResponse(
                        user.GenerateAuthToken(_authTokenGenerator),
                        user.GenerateRefreshToken(_authTokenGenerator)
                    ).ToErrorOr(),
                error => error
            );

    private async Task<ErrorOr<User>> GetUserByLoginAsync(string userLogin, CancellationToken cancellationToken) =>
        await _dbContext
            .Users.AsNoTracking()
            .FirstOrErrorAsync(user => user.Login == userLogin, UserErrors.UserInvalidCredentials, cancellationToken);
}
