namespace Application.Features.Accounts.Commands.RenameAccountOperation;

public sealed record RenameAccountOperationResponse(
    Guid Id,
    Guid AccountId,
    string AccountName,
    string Description,
    decimal Amount,
    decimal PreviousBalance,
    decimal NextBalance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
