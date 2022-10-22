using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Personal.Budget.Api.Domain;

namespace Personal.Budget.Api.Persistence.Configurations;

public class OperationTagConfiguration : IEntityTypeConfiguration<OperationTag>
{
    public void Configure(EntityTypeBuilder<OperationTag> builder)
    {
        builder.HasIndex(p => new {p.Name, p.OwnerId}).IsUnique();

        builder.Property(t => t.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.Color).IsRequired();

        builder
            .HasOne(p => p.Owner)
            .WithMany(p => p.Tags)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}