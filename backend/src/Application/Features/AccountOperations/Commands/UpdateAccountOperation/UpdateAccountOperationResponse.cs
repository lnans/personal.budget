namespace Application.Features.AccountOperations.Commands.UpdateAccountOperation;

public record UpdateAccountOperationResponse(
    Guid Id,
    Guid AccountId,
    string AccountName,
    string Description,
    decimal Amount,
    decimal PreviousBalance,
    decimal NextBalance,
    bool IsRecurring,
    IReadOnlyList<UpdateAccountOperationTagResponse> Tags,
    DateTimeOffset OperationDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public sealed record UpdateAccountOperationTagResponse(Guid Id, string Name, string Color);
