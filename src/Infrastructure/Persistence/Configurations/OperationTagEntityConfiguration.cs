using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OperationTagEntityConfiguration : IEntityTypeConfiguration<OperationTag>
{
    public void Configure(EntityTypeBuilder<OperationTag> builder)
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