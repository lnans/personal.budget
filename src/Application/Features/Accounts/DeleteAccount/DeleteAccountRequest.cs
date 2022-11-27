using MediatR;

namespace Application.Features.Accounts.DeleteAccount;

public sealed record DeleteAccountRequest : IRequest<Unit>
{
    public Guid Id { get; set; }
}