using Domain.Accounts;

namespace Application.Features.Accounts.Commands.RenameAccount;

public sealed record RenameAccountResponse(
    Guid Id,
    string Name,
    AccountType Type,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
