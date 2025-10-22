namespace Application.Features.Accounts.Commands.RenameAccount;

public sealed record RenameAccountResponse(
    Guid Id,
    string Name,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
