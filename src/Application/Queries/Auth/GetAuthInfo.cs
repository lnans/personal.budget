using System.Security.Authentication;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Auth;

public record GetAuthInfoQuery : IRequest<GetAuthInfoResponse>;

public record GetAuthInfoResponse(string Id, string Username);

public class GetAuthInfo : IRequestHandler<GetAuthInfoQuery, GetAuthInfoResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetAuthInfo(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<GetAuthInfoResponse> Handle(GetAuthInfoQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null) throw new AuthenticationException();

        return new GetAuthInfoResponse(user.Id, user.Username);
    }
}