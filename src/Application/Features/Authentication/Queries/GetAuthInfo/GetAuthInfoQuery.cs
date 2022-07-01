using System.Security.Authentication;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Authentication.Queries.GetAuthInfo;

public record GetAuthInfoRequest : IRequest<AuthenticationInfoDto>;

public class GetAuthInfoQueryHandler : IRequestHandler<GetAuthInfoRequest, AuthenticationInfoDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public GetAuthInfoQueryHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<AuthenticationInfoDto> Handle(GetAuthInfoRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null) throw new AuthenticationException();

        return new AuthenticationInfoDto {Id = user.Id, Username = user.Username};
    }
}