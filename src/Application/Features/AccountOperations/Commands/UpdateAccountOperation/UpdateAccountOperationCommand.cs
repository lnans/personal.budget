using Application.Interfaces;

namespace Application.Features.AccountOperations.Commands.UpdateAccountOperation;

public sealed class UpdateAccountOperationCommand : ICommand<UpdateAccountOperationResponse>
{
    public Guid AccountId { get; set; }
    public Guid OperationId { get; set; }
    public required decimal Amount { get; set; }
    public required string Description { get; set; }
}
