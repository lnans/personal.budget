using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TagEntityConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.Color).IsRequired();

        builder.HasIndex(p => new {p.Name, p.OwnerId}).IsUnique();

        builder
            .HasOne(p => p.Owner)
            .WithMany(p => p.Tags)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}