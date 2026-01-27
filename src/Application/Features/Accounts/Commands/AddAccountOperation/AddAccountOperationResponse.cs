namespace Application.Features.Accounts.Commands.AddAccountOperation;

public sealed record AddAccountOperationResponse(
    Guid Id,
    Guid AccountId,
    string Description,
    decimal Amount,
    decimal PreviousBalance,
    decimal NextBalance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
