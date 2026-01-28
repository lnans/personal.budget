using ErrorOr;
using MediatR;

namespace Application.Features.Accounts.Commands.PatchAccount;

public sealed class PatchAccountCommand : IRequest<ErrorOr<PatchAccountResponse>>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Bank { get; set; }
    public decimal? InitialBalance { get; set; }
}
