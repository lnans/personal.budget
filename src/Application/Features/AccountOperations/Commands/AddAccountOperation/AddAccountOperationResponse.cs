namespace Application.Features.AccountOperations.Commands.AddAccountOperation;

public sealed record AddAccountOperationResponse(
    Guid Id,
    Guid AccountId,
    string AccountName,
    string Description,
    decimal Amount,
    decimal PreviousBalance,
    decimal NextBalance,
    bool IsRecurring,
    IReadOnlyList<AddAccountOperationTagResponse> Tags,
    DateTimeOffset OperationDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public sealed record AddAccountOperationTagResponse(Guid Id, string Name, string Color);
