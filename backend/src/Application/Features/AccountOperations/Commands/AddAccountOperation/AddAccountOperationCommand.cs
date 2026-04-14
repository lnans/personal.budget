using Application.Interfaces;

namespace Application.Features.AccountOperations.Commands.AddAccountOperation;

public sealed record AddAccountOperationCommand(
    Guid AccountId,
    string Description,
    decimal Amount,
    bool IsRecurring = false,
    DateTimeOffset? OperationDate = null,
    IReadOnlyList<Guid>? TagIds = null
) : ICommand<AddAccountOperationResponse>;
