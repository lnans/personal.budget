namespace Application.Features.Accounts.Commands.DeleteAccountOperation;

public sealed record DeleteAccountOperationResponse(
    Guid Id,
    Guid AccountId,
    string Description,
    decimal Amount,
    decimal PreviousBalance,
    decimal NextBalance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset DeletedAt
);
