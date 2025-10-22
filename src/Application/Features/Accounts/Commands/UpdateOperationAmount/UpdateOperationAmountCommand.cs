using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.UpdateOperationAmount;

public sealed class UpdateOperationAmountCommand : IRequest<ErrorOr<UpdateOperationAmountResponse>>
{
    public Guid AccountId { get; set; }
    public Guid OperationId { get; set; }
    public required decimal Amount { get; set; }
}
