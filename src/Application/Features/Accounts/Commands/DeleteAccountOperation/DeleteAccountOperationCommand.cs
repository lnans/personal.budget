using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.DeleteAccountOperation;

public sealed class DeleteAccountOperationCommand : IRequest<ErrorOr<DeleteAccountOperationResponse>>
{
    public Guid AccountId { get; set; }
    public Guid OperationId { get; set; }
}
