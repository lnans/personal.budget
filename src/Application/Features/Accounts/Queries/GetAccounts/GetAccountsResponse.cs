namespace Application.Features.Accounts.Queries.GetAccounts;

public sealed record GetAccountsResponse(
    Guid Id,
    string Name,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
