namespace Domain.Entities;

public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Hash { get; set; }

    public ICollection<Account> Accounts { get; set; }
    public ICollection<Tag> Tags { get; set; }
}