namespace Application.Features.Accounts.Commands.UpdateAccountOperationAmount;

public sealed record UpdateAccountOperationAmountResponse(
    Guid Id,
    Guid AccountId,
    string Description,
    decimal Amount,
    decimal PreviousBalance,
    decimal NextBalance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
