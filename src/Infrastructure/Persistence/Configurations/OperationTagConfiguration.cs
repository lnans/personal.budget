using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OperationTagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasIndex(tag => new { tag.Name, tag.OwnerId }).IsUnique();

        builder.Property(tag => tag.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(tag => tag.Name).IsRequired();
        builder.Property(tag => tag.OwnerId).IsRequired();
        builder.Property(tag => tag.Color).IsRequired();

        builder.HasMany(tag => tag.Operations).WithMany(operation => operation.Tags);
    }
}