using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OperationEntityConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.Amount).IsRequired();
        builder.Property(p => p.OperationDate).IsRequired();
        builder.Property(p => p.IsIncome).IsRequired();

        builder
            .HasOne(p => p.Account)
            .WithMany(p => p.Operations)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}