using Domain.Enums;
using MediatR;

namespace Application.Features.Accounts.CreateAccount;

public sealed record CreateAccountRequest : IRequest<Guid>
{
    public string? Name { get; init; }
    public string? Bank { get; init; }
    public AccountType Type { get; init; }
    public decimal InitialBalance { get; init; }
}