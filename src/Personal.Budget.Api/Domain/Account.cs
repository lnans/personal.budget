using Personal.Budget.Api.Domain.Enums;

namespace Personal.Budget.Api.Domain;

public sealed class Account
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = default!;
    public string Bank { get; set; } = default!;
    public decimal InitialBalance { get; set; }
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public DateTime CreationDate { get; set; }
    public bool Archived { get; set; }

    public User Owner { get; set; } = default!;
    public ICollection<Operation> Operations { get; set; } = default!;
}