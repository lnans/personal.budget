using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.OperationTags.Queries.GetAllOperationTags;

public record GetAllOperationTagsRequest : IRequest<IEnumerable<OperationTagDto>>
{
    public string Name { get; init; }
}

public class GetAllOperationTagsCommandHandler : IRequestHandler<GetAllOperationTagsRequest, IEnumerable<OperationTagDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public GetAllOperationTagsCommandHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<IEnumerable<OperationTagDto>> Handle(GetAllOperationTagsRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var operationTags = _dbContext.OperationTags.Where(op => op.OwnerId == userId);

        if (!string.IsNullOrWhiteSpace(request.Name))
            operationTags = operationTags.Where(op => op.Name.ToLower().Contains(request.Name.ToLower()));

        return await operationTags
            .Select(op => new OperationTagDto {Id = op.Id, Name = op.Name, Color = op.Color})
            .ToListAsync(cancellationToken);
    }
}