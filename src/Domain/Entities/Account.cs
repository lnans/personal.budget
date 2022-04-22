using Domain.Enums;

namespace Domain.Entities;

public class Account
{
    public string Id { get; set; }
    public string OwnerId { get; set; }
    public string Name { get; set; }
    public string Bank { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal Balance { get; set; }
    public string Icon { get; set; }
    public AccountType Type { get; set; }
    public DateTime CreationDate { get; set; }
    public bool Archived { get; set; }

    public virtual User Owner { get; set; }
    public virtual ICollection<Operation> Operations { get; set; }
}