namespace Application.Queries.Operations;

public record GetAllOperationsRequest(
    string AccountId,
    string Description,
    string[] TagIds,
    OperationType? Type,
    int PageSize = 25,
    int Skip = 0) : IRequest<GetAllOperationsPaginatedResponse>;

public record GetAllOperationsResponse(
    string Id,
    string Description,
    string TagId,
    string TagName,
    string TagColor,
    OperationType Type,
    string AccountId,
    string AccountName,
    decimal Amount,
    DateTime CreationDate,
    DateTime? ExecutionDate);

public record GetAllOperationsPaginatedResponse(int Total, Dictionary<string, List<GetAllOperationsResponse>> OperationsByDays);

public class GetAllOperations : IRequestHandler<GetAllOperationsRequest, GetAllOperationsPaginatedResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetAllOperations(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<GetAllOperationsPaginatedResponse> Handle(GetAllOperationsRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        var query = _dbContext
            .Operations
            .Include(o => o.Account)
            .Include(o => o.Tag)
            .Where(o => o.Account.OwnerId == userId)
            .OrderByDescending(o => o.ExecutionDate ?? DateTime.MaxValue)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.AccountId)) query = query.Where(o => o.Account.Id == request.AccountId);

        if (!string.IsNullOrWhiteSpace(request.Description)) query = query.Where(o => o.Description.ToLower().Contains(request.Description.ToLower()));

        if (request.TagIds is not null && request.TagIds.Length > 0) query = query.Where(o => request.TagIds.Contains(o.Tag.Id));

        if (request.Type.HasValue) query = query.Where(o => o.Type == request.Type);

        var totalSize = await query.CountAsync(cancellationToken);
        var list = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .Select(o => new GetAllOperationsResponse(
                o.Id,
                o.Description,
                o.Tag != null ? o.Tag.Id : null,
                o.Tag != null ? o.Tag.Name : null,
                o.Tag != null ? o.Tag.Color : null,
                o.Type,
                o.Account.Id,
                o.Account.Name,
                o.Amount,
                o.CreationDate,
                o.ExecutionDate))
            .ToListAsync(cancellationToken);

        // Group result by days in a dictionary
        var operationsByDays = new Dictionary<string, List<GetAllOperationsResponse>>();
        foreach (var operation in list)
        {
            var strDateOnly = operation.ExecutionDate.HasValue ? operation.ExecutionDate.Value.ToMidnightIsoString() : "none";
            if (!operationsByDays.ContainsKey(strDateOnly)) operationsByDays.Add(strDateOnly, new List<GetAllOperationsResponse>());
            operationsByDays[strDateOnly].Add(operation);
        }

        return new GetAllOperationsPaginatedResponse(totalSize, operationsByDays);
    }
}