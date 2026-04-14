using Application.Interfaces;
using Application.Models.Pagination;

namespace Application.Features.AccountOperations.Queries.GetPaginatedAccountOperations;

public sealed record GetPaginatedAccountOperationsQuery(
    Guid? AccountId = null,
    int? PageNumber = null,
    int? PageSize = null
) : PaginatedQuery(PageNumber, PageSize), IQuery<PaginatedList<GetPaginatedAccountOperationsResponse>>;
