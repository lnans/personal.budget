using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.Property(operation => operation.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(operation => operation.Description).IsRequired();
        builder.Property(operation => operation.Amount).IsRequired();
        builder.Property(operation => operation.CreationDate).IsRequired();
        builder.Property(operation => operation.Type).HasConversion<string>().IsRequired();
        builder.Property(operation => operation.OwnerId).IsRequired();

        builder
            .HasOne(p => p.Account)
            .WithMany(p => p.Operations)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasMany(operation => operation.Tags).WithMany(tag => tag.Operations);
    }
}