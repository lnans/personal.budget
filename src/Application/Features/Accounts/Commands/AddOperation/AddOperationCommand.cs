using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.AddOperation;

public sealed class AddOperationCommand : IRequest<ErrorOr<AddOperationResponse>>
{
    public Guid AccountId { get; set; }
    public required string Description { get; set; }
    public required decimal Amount { get; set; }
}
