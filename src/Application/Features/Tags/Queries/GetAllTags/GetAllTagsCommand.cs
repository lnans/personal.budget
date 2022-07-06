using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tags.Queries.GetAllTags;

public record GetAllTagsRequest : IRequest<IEnumerable<TagDto>>
{
    public string Name { get; init; }
}

public class GetAllTagsCommandHandler : IRequestHandler<GetAllTagsRequest, IEnumerable<TagDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public GetAllTagsCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<IEnumerable<TagDto>> Handle(GetAllTagsRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var tags = _dbContext.Tags.Where(op => op.OwnerId == userId);

        if (!string.IsNullOrWhiteSpace(request.Name))
            tags = tags.Where(op => op.Name.ToLower().Contains(request.Name.ToLower()));

        return await tags
            .Select(op => new TagDto {Id = op.Id, Name = op.Name, Color = op.Color})
            .ToListAsync(cancellationToken);
    }
}