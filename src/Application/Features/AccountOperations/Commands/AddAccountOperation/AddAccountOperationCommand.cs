using Application.Interfaces;

namespace Application.Features.AccountOperations.Commands.AddAccountOperation;

public sealed record AddAccountOperationCommand(Guid AccountId, string Description, decimal Amount)
    : ICommand<AddAccountOperationResponse>;
