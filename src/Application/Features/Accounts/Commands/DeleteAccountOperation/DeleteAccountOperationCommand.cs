using Application.Interfaces;

namespace Application.Features.Accounts.Commands.DeleteAccountOperation;

public sealed class DeleteAccountOperationCommand : ICommand<DeleteAccountOperationResponse>
{
    public Guid AccountId { get; set; }
    public Guid OperationId { get; set; }
}
