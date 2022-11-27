using Domain.Enums;

namespace Application.Features.Accounts.GetAccounts;

public sealed record GetAccountsResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Bank { get; init; } = null!;
    public AccountType Type { get; init; }
    public decimal Balance { get; init; }
    public bool Archived { get; init; }
    public DateTime CreationDate { get; init; }
}