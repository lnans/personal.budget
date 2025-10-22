namespace Application.Features.Accounts.Commands.UpdateOperationAmount;

public sealed record UpdateOperationAmountResponse(
    Guid Id,
    Guid AccountId,
    string Description,
    decimal Amount,
    decimal PreviousBalance,
    decimal NextBalance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
