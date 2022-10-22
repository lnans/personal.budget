using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Personal.Budget.Api.Domain;

namespace Personal.Budget.Api.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasIndex(p => new {p.Name, p.Bank, p.OwnerId}).IsUnique();

        builder.Property(t => t.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.Bank).IsRequired();
        builder.Property(p => p.InitialBalance).IsRequired();
        builder.Property(p => p.Balance).IsRequired();
        builder.Property(p => p.Type).HasConversion<string>().IsRequired();
        builder.Property(p => p.CreationDate).IsRequired();
        builder.Property(p => p.Archived).IsRequired();

        builder
            .HasOne(p => p.Owner)
            .WithMany(p => p.Accounts)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}