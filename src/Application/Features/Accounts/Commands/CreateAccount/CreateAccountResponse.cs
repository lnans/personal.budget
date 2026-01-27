using Domain.Accounts;

namespace Application.Features.Accounts.Commands.CreateAccount;

public sealed record CreateAccountResponse(
    Guid Id,
    string Name,
    string Bank,
    AccountType Type,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
