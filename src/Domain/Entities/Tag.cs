namespace Domain.Entities;

public sealed class Tag
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; } = null!;
    public string Name { get; set; } = default!;
    public string Color { get; set; } = default!;

    public ICollection<Operation>? Operations { get; set; }
}