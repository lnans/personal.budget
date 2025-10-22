using Domain.Accounts;

namespace Application.Features.Accounts.Queries.GetAccounts;

public sealed record GetAccountsResponse(
    Guid Id,
    string Name,
    AccountType Type,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
