using Application.Interfaces;

namespace Application.Features.AccountOperations.Commands.UpdateAccountOperation;

public sealed record UpdateAccountOperationCommand(Guid AccountId, Guid OperationId, decimal Amount, string Description)
    : ICommand<UpdateAccountOperationResponse>;
