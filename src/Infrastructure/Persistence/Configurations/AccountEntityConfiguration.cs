using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AccountEntityConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.Bank).IsRequired();
        builder.Property(p => p.InitialBalance).IsRequired();
        builder.Property(p => p.Balance).IsRequired();
        builder.Property(p => p.Type).HasConversion<string>().IsRequired();
        builder.Property(p => p.CreationDate).IsRequired();
        builder.Property(p => p.Archived).IsRequired();

        builder.HasIndex(p => new {p.Name, p.Bank, p.OwnerId}).IsUnique();

        builder
            .HasOne(p => p.Owner)
            .WithMany(p => p.Accounts)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}