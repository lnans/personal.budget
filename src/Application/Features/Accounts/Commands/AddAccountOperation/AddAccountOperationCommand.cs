using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.AddAccountOperation;

public sealed class AddAccountOperationCommand : IRequest<ErrorOr<AddAccountOperationResponse>>
{
    public Guid AccountId { get; set; }
    public required string Description { get; set; }
    public required decimal Amount { get; set; }
}
