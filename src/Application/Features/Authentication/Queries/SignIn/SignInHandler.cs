using Application.Interfaces;
using Domain.Users;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Authentication.Queries.SignIn;

public sealed class SignInHandler : IRequestHandler<SignInQuery, ErrorOr<SignInResponse>>
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

    public async Task<ErrorOr<SignInResponse>> Handle(SignInQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Login == request.Login, cancellationToken);
        if (user is null)
        {
            return UserErrors.UserInvalidCredentials;
        }

        var passwordCheckResult = user.VerifyPassword(request.Password, _passwordHasher);
        if (passwordCheckResult.IsError)
        {
            return UserErrors.UserInvalidCredentials;
        }

        var bearer = user.GenerateAuthToken(_authTokenGenerator);
        return new SignInResponse(bearer);
    }
}
