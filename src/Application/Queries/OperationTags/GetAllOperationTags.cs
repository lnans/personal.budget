using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.OperationTags;

public record GetAllOperationTagsRequest(string Name) : IRequest<IEnumerable<GetAllOperationTagsResponse>>;
public record GetAllOperationTagsResponse(string Id, string Name, string Color);

public class GetAllOperationTags : IRequestHandler<GetAllOperationTagsRequest, IEnumerable<GetAllOperationTagsResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetAllOperationTags(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<IEnumerable<GetAllOperationTagsResponse>> Handle(GetAllOperationTagsRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var operationTags = _dbContext.OperationTags.Where(op => op.OwnerId == userId);

        if (!string.IsNullOrWhiteSpace(request.Name))
            operationTags = operationTags.Where(op => op.Name.ToLower().Contains(request.Name.ToLower()));

        return await operationTags
            .Select(op => new GetAllOperationTagsResponse(op.Id, op.Name, op.Color))
            .ToListAsync(cancellationToken);
    }
}