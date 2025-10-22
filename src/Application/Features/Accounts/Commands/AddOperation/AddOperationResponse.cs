namespace Application.Features.Accounts.Commands.AddOperation;

public sealed record AddOperationResponse(
    Guid Id,
    string Name,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
