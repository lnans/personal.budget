using Application.Extensions;
using Application.Interfaces;
using Domain.Users;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Authentication.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, GetCurrentUserResponse>
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
    ) =>
        await _dbContext
            .Users.AsNoTracking()
            .Where(u => u.Id == _authContext.CurrentUserId)
            .Select(u => new GetCurrentUserResponse(u.Id, u.Login))
            .FirstOrErrorAsync(UserErrors.UserNotFound, cancellationToken);
}
