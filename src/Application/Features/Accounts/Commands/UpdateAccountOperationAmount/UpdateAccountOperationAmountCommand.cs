using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.UpdateAccountOperationAmount;

public sealed class UpdateAccountOperationAmountCommand : IRequest<ErrorOr<UpdateAccountOperationAmountResponse>>
{
    public Guid AccountId { get; set; }
    public Guid OperationId { get; set; }
    public required decimal Amount { get; set; }
}
