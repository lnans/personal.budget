using Application.Interfaces;
using Application.Models;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Queries.GetPaginatedAccountOperations;

public sealed class GetPaginatedAccountOperationsHandler
    : IRequestHandler<GetPaginatedAccountOperationsQuery, ErrorOr<PaginatedList<GetPaginatedAccountOperationsResponse>>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;

    public GetPaginatedAccountOperationsHandler(IAppDbContext dbContext, IAuthContext authContext)
    {
        _dbContext = dbContext;
        _authContext = authContext;
    }

    public async Task<ErrorOr<PaginatedList<GetPaginatedAccountOperationsResponse>>> Handle(
        GetPaginatedAccountOperationsQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = _dbContext.AccountOperations.Where(op => op.Account.UserId == _authContext.CurrentUserId);

        if (request.AccountId.HasValue)
        {
            query = query.Where(op => op.AccountId == request.AccountId.Value);
        }

        var orderedQuery = query.OrderByDescending(op => op.CreatedAt);

        var totalCount = await orderedQuery.CountAsync(cancellationToken);

        var items = await orderedQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(op => new GetPaginatedAccountOperationsResponse(
                op.Id,
                op.AccountId,
                op.Account.Name,
                op.Description,
                op.Amount,
                op.PreviousBalance,
                op.NextBalance,
                op.CreatedAt,
                op.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<GetPaginatedAccountOperationsResponse>(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount
        );
    }
}
