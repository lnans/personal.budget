namespace Personal.Budget.Api.Domain;

public sealed class OperationTag
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = default!;
    public string Color { get; set; } = default!;

    public User Owner { get; set; } = default!;
}