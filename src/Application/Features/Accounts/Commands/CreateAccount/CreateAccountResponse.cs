namespace Application.Features.Accounts.Commands.CreateAccount;

public sealed record CreateAccountResponse(
    Guid Id,
    string Name,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
