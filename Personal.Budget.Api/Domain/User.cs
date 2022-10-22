namespace Personal.Budget.Api.Domain;

public sealed class User
{
    public Guid Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Hash { get; set; } = default!;

    public ICollection<Account> Accounts { get; set; } = default!;
    public ICollection<OperationTag> Tags { get; set; } = default!;
}