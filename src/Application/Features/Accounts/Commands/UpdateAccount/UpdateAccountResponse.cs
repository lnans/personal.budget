using Domain.Accounts;

namespace Application.Features.Accounts.Commands.UpdateAccount;

public sealed record UpdateAccountResponse(
    Guid Id,
    string Name,
    string Bank,
    AccountType Type,
    decimal InitialBalance,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
