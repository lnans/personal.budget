using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.GetTags;

internal sealed class GetTagsRequestHandler : IRequestHandler<GetTagsRequest, IEnumerable<GetTagsResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthContext _authContext;

    public GetTagsRequestHandler(IApplicationDbContext dbContext, IAuthContext authContext)
    {
        _dbContext = dbContext;
        _authContext = authContext;
    }

    public async Task<IEnumerable<GetTagsResponse>> Handle(GetTagsRequest request, CancellationToken cancellationToken)
    {
        var userId = _authContext.GetAuthenticatedUserId();
        var tags = _dbContext.Tags.Where(tag => tag.OwnerId == userId);

        return await tags
            .Select(tag => new GetTagsResponse { Id = tag.Id, Name = tag.Name, Color = tag.Color })
            .ToListAsync(cancellationToken);
    }
}