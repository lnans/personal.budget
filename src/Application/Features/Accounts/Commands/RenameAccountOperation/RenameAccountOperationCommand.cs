using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.RenameAccountOperation;

public sealed class RenameAccountOperationCommand : IRequest<ErrorOr<RenameAccountOperationResponse>>
{
    public Guid AccountId { get; set; }
    public Guid OperationId { get; set; }
    public required string Description { get; set; }
}
