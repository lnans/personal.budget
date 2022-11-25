using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasIndex(account => new { account.Name, account.Bank, account.OwnerId }).IsUnique();

        builder.Property(account => account.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(account => account.OwnerId).IsRequired();
        builder.Property(account => account.Name).IsRequired();
        builder.Property(account => account.Bank).IsRequired();
        builder.Property(account => account.InitialBalance).IsRequired();
        builder.Property(account => account.Balance).IsRequired();
        builder.Property(account => account.Type).HasConversion<string>().IsRequired();
        builder.Property(account => account.CreationDate).IsRequired();
        builder.Property(account => account.Archived).IsRequired();
    }
}