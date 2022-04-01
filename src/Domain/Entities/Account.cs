using Domain.Enums;

namespace Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public User Owner { get; set; }
    public string Name { get; set; }
    public decimal Balance { get; set; }
    public string Icon { get; set; }
    public AccountType Type { get; set; }
    public DateTime CreationDate { get; set; }

    public virtual ICollection<Operation> Operations { get; set; }
}