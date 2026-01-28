using Domain.Accounts;

namespace Application.Features.Accounts.Commands.PatchAccount;

public sealed record PatchAccountResponse(
    Guid Id,
    string Name,
    string Bank,
    AccountType Type,
    decimal InitialBalance,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
