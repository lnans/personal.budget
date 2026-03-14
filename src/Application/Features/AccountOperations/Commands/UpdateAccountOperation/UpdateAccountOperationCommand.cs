using Application.Interfaces;

namespace Application.Features.AccountOperations.Commands.UpdateAccountOperation;

public sealed record UpdateAccountOperationCommand(
    Guid AccountId,
    Guid OperationId,
    decimal Amount,
    string Description,
    bool IsRecurring = false,
    DateTimeOffset? OperationDate = null
) : ICommand<UpdateAccountOperationResponse>;
