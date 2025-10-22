using Domain.Accounts;

namespace Application.Features.Accounts.Commands.AddOperation;

public sealed record AddOperationResponse(
    Guid Id,
    string Name,
    AccountType Type,
    decimal Balance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
