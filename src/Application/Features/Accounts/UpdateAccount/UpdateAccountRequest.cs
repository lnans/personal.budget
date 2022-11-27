using MediatR;

namespace Application.Features.Accounts.UpdateAccount;

public sealed record UpdateAccountRequest : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string? Name { get; init; }
    public string? Bank { get; init; }
}