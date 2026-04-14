using Application.Interfaces;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.Queries.GetTags;

public sealed class GetTagsHandler : IQueryHandler<GetTagsQuery, List<GetTagsResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;

    public GetTagsHandler(IAppDbContext dbContext, IAuthContext authContext)
    {
        _dbContext = dbContext;
        _authContext = authContext;
    }

    public async Task<ErrorOr<List<GetTagsResponse>>> Handle(
        GetTagsQuery request,
        CancellationToken cancellationToken
    ) =>
        await _dbContext
            .Tags.AsNoTracking()
            .Where(tag => tag.UserId == _authContext.CurrentUserId)
            .Select(tag => new GetTagsResponse(tag.Id, tag.Name, tag.Color, tag.CreatedAt, tag.UpdatedAt))
            .ToListAsync(cancellationToken);
}
