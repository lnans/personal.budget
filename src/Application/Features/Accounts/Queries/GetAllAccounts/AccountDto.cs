using Domain.Enums;

namespace Application.Features.Accounts.Queries.GetAllAccounts;

public record AccountDto
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Bank { get; init; }
    public string Icon { get; init; }
    public AccountType Type { get; init; }
    public decimal Balance { get; init; }
    public bool Archived { get; init; }
    public DateTime CreationDate { get; init; }
}