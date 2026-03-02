using Application.Interfaces;

namespace Application.Features.AccountOperations.Commands.DeleteAccountOperation;

public sealed record DeleteAccountOperationCommand(Guid AccountId, Guid OperationId)
    : ICommand<DeleteAccountOperationResponse>;
