using Domain.Enums;

namespace Domain.Entities;

public sealed class Account
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; } = null!;
    public string Name { get; set; } = default!;
    public string Bank { get; set; } = default!;
    public decimal InitialBalance { get; set; }
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public DateTime CreationDate { get; set; }
    public bool Archived { get; set; }

    public ICollection<Operation>? Operations { get; set; }
}