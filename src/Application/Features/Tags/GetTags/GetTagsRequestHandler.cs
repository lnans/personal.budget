using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.GetTags;

internal sealed class GetTagsRequestHandler : IRequestHandler<GetTagsRequest, IEnumerable<GetTagsResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetTagsRequestHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<IEnumerable<GetTagsResponse>> Handle(GetTagsRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
        var tags = _dbContext.Tags.Where(op => op.OwnerId == userId);

        return await tags
            .Select(op => new GetTagsResponse { Id = op.Id, Name = op.Name, Color = op.Color })
            .ToListAsync(cancellationToken);
    }
}