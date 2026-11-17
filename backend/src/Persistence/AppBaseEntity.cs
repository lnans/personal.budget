using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Personal.Budget.App.Persistence;

internal abstract class AppBaseEntity<TSelf> : IEntityTypeConfiguration<TSelf>
    where TSelf : AppBaseEntity<TSelf>
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; protected set; }
    public DateTimeOffset? DeletedAt { get; protected set; }

    protected AppBaseEntity() { }

    protected AppBaseEntity(DateTimeOffset createdAt)
    {
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public virtual void Configure(EntityTypeBuilder<TSelf> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedNever().IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.CreatedAt);

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}
