namespace Api.Contracts.AccountOperations;

public sealed record AddAccountOperationRequest(
    string Description,
    decimal Amount,
    DateTimeOffset? OperationDate = null
);
