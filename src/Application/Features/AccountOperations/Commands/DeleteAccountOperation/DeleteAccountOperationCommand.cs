using Application.Interfaces;

namespace Application.Features.AccountOperations.Commands.DeleteAccountOperation;

public sealed class DeleteAccountOperationCommand : ICommand<DeleteAccountOperationResponse>
{
    public Guid AccountId { get; set; }
    public Guid OperationId { get; set; }
}
