namespace Api.Contracts.AccountOperations;

public sealed record UpdateAccountOperationRequest(
    decimal Amount,
    string Description,
    DateTimeOffset? OperationDate = null
);
