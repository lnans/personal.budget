namespace Api.Contracts.AccountOperations;

public sealed record AddAccountOperationRequest(
    string Description,
    decimal Amount,
    bool IsRecurring = false,
    DateTimeOffset? OperationDate = null
);
