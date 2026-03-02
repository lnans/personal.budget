using Application.Models;

namespace Application.Features.AccountOperations.Queries.GetPaginatedAccountOperations;

public sealed class GetPaginatedAccountOperationsQuery : PaginatedQuery<GetPaginatedAccountOperationsResponse>
{
    public Guid? AccountId { get; init; }
}
