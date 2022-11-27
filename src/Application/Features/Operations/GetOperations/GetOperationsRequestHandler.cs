using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Operations.GetOperations;

internal sealed class GetOperationsRequestHandler : IRequestHandler<GetOperationsRequest, InfiniteDataList<GetOperationsResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetOperationsRequestHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<InfiniteDataList<GetOperationsResponse>> Handle(GetOperationsRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
        var operations = _dbContext.Operations
            .Include(o => o.Account)
            .Include(o => o.Tags)
            .Where(o => o.Account.OwnerId == userId)
            .OrderByDescending(o => o.CreationDate)
            .AsNoTracking();

        if (request.AccountId.HasValue)
            operations = operations.Where(o => o.Account.Id == request.AccountId);

        if (!string.IsNullOrWhiteSpace(request.Description))
            operations = operations.Where(o => o.Description.ToLower().Contains(request.Description.ToLower()));

        if (request.TagIds != null && request.TagIds.Any())
            operations = operations.Where(o => o.Tags!.Any(t => request.TagIds.Contains(t.Id)));

        if (request.Type.HasValue)
            operations = operations.Where(o => o.Type == request.Type);

        return await operations
            .Select(o => new GetOperationsResponse
            {
                Id = o.Id,
                Description = o.Description,
                Tags = o.Tags != null
                    ? o.Tags.Select(t => new OperationTagData
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Color = t.Color
                    })
                    : Array.Empty<OperationTagData>(),
                Account = new OperationAccountData
                {
                    Id = o.Account.Id,
                    Name = o.Account.Name
                },
                Type = o.Type,
                Amount = o.Amount,
                CreationDate = o.CreationDate,
                ExecutionDate = o.ExecutionDate
            })
            .ToInfiniteDataListAsync(request.Cursor, request.PageSize, cancellationToken);
    }
}