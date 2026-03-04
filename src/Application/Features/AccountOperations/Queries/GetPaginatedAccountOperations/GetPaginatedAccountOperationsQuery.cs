using Application.Interfaces;
using Application.Models;

namespace Application.Features.AccountOperations.Queries.GetPaginatedAccountOperations;

public sealed record GetPaginatedAccountOperationsQuery(
    Guid? AccountId = null,
    int PageNumber = PaginationConstants.DefaultPageNumber,
    int PageSize = PaginationConstants.DefaultPageSize
) : PaginatedQuery(PageNumber, PageSize), IQuery<PaginatedList<GetPaginatedAccountOperationsResponse>>;
