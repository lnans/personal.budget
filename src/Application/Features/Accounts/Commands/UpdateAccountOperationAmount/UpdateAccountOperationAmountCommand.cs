using Application.Interfaces;

namespace Application.Features.Accounts.Commands.UpdateAccountOperationAmount;

public sealed class UpdateAccountOperationAmountCommand : ICommand<UpdateAccountOperationAmountResponse>
{
    public Guid AccountId { get; set; }
    public Guid OperationId { get; set; }
    public required decimal Amount { get; set; }
}
