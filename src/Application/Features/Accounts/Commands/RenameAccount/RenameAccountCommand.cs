using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.RenameAccount;

public sealed class RenameAccountCommand : IRequest<ErrorOr<RenameAccountResponse>>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
