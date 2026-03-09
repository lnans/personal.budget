using Application.Extensions;
using Application.Interfaces;
using Application.Models.Pagination;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AccountOperations.Queries.GetPaginatedAccountOperations;

public sealed class GetPaginatedAccountOperationsHandler
    : IQueryHandler<GetPaginatedAccountOperationsQuery, PaginatedList<GetPaginatedAccountOperationsResponse>>
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
    ) =>
        await _dbContext
            .AccountOperations.AsNoTracking()
            .Where(operation => operation.Account.UserId == _authContext.CurrentUserId)
            .Where(operation => !request.AccountId.HasValue || operation.AccountId == request.AccountId!.Value)
            .OrderByDescending(operation => operation.CreatedAt)
            .ToPaginatedListOrErrorAsync(
                operation => new GetPaginatedAccountOperationsResponse(
                    operation.Id,
                    operation.AccountId,
                    operation.Account.Name,
                    operation.Description,
                    operation.Amount,
                    operation.PreviousBalance,
                    operation.NextBalance,
                    operation.OperationDate,
                    operation.CreatedAt,
                    operation.UpdatedAt
                ),
                request.PageNumber,
                request.PageSize,
                cancellationToken
            );
}
