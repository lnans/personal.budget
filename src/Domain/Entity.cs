namespace Domain;

public abstract class Entity
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; protected set; } = DateTimeOffset.UtcNow;
}
