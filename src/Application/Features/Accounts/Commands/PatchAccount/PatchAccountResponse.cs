namespace Application.Features.Accounts.Commands.PatchAccount;

public sealed record PatchAccountResponse(
    Guid Id,
    string Name,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
