using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.OperationTag;

public record GetAllOperationTagQuery : IRequest<IEnumerable<GetAllOperationTagResponse>>;
public record GetAllOperationTagResponse(string Id, string Name, string Color);

public class GetAllOperationTagQueryHandler : IRequestHandler<GetAllOperationTagQuery, IEnumerable<GetAllOperationTagResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllOperationTagQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<GetAllOperationTagResponse>> Handle(GetAllOperationTagQuery request, CancellationToken cancellationToken)
    {
        var operationTags = await _dbContext.OperationTags.ToListAsync(cancellationToken);
        return operationTags.Select(op => new GetAllOperationTagResponse(op.Id, op.Name, op.Color));
    }
}