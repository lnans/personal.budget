using Application.Interfaces;

namespace Application.Features.AccountOperations.Commands.AddAccountOperation;

public sealed class AddAccountOperationCommand : ICommand<AddAccountOperationResponse>
{
    public Guid AccountId { get; set; }
    public required string Description { get; set; }
    public required decimal Amount { get; set; }
}
