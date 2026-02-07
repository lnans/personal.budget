namespace Application.Features.Accounts.Queries.GetPaginatedAccountOperations;

public sealed record GetPaginatedAccountOperationsResponse(
    Guid Id,
    Guid AccountId,
    string AccountName,
    string Description,
    decimal Amount,
    decimal PreviousBalance,
    decimal NextBalance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
