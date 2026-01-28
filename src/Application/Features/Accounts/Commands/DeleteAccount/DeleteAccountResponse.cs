using Domain.Accounts;

namespace Application.Features.Accounts.Commands.DeleteAccount;

public sealed record DeleteAccountResponse(
    Guid Id,
    string Name,
    string Bank,
    AccountType Type,
    decimal InitialBalance,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset DeletedAt
);
