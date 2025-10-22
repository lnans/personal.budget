using Application.Interfaces;
using Domain.Users;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Authentication.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, ErrorOr<GetCurrentUserResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;

    public GetCurrentUserHandler(IAppDbContext dbContext, IAuthContext authContext)
    {
        _dbContext = dbContext;
        _authContext = authContext;
    }

    public async Task<ErrorOr<GetCurrentUserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = _authContext.CurrentUserId;

        var user = await _dbContext
            .Users.Where(u => u.Id == userId)
            .Select(u => new GetCurrentUserResponse(u.Id, u.Login))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return UserErrors.UserNotFound;
        }

        return user;
    }
}
