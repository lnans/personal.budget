namespace Api.Contracts.AccountOperations;

public sealed record UpdateAccountOperationRequest(
    decimal Amount,
    string Description,
    bool IsRecurring = false,
    DateTimeOffset? OperationDate = null
);
