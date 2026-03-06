namespace Application.Features.AccountOperations.Commands.AddAccountOperation;

public sealed record AddAccountOperationResponse(
    Guid Id,
    Guid AccountId,
    string AccountName,
    string Description,
    decimal Amount,
    decimal PreviousBalance,
    decimal NextBalance,
    DateTimeOffset OperationDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
