using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TransactionEntityConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.Amount).IsRequired();
        builder.Property(p => p.CreationDate).IsRequired();
        builder.Property(p => p.Type).HasConversion<string>().IsRequired();
        builder.Property(p => p.CreatedById).IsRequired();

        builder
            .HasOne(p => p.Account)
            .WithMany(p => p.Transactions)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}