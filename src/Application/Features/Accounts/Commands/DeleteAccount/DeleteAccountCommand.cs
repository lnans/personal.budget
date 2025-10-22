using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.DeleteAccount;

public sealed class DeleteAccountCommand : IRequest<ErrorOr<DeleteAccountResponse>>
{
    public Guid Id { get; set; }
}
