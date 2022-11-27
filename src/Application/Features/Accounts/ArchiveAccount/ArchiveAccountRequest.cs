using MediatR;

namespace Application.Features.Accounts.ArchiveAccount;

public sealed record ArchiveAccountRequest : IRequest<Unit>
{
    public Guid Id { get; set; }
    public bool Archived { get; set; }
}