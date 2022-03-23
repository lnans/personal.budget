namespace Domain.Entities;

public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Hash { get; set; }

    public virtual ICollection<Account> Accounts { get; set; }
    public virtual ICollection<OperationTag> Tags { get; set; }
}