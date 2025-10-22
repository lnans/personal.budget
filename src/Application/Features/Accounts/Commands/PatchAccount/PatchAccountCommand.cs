using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.PatchAccount;

public sealed class PatchAccountCommand : IRequest<ErrorOr<PatchAccountResponse>>
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}
