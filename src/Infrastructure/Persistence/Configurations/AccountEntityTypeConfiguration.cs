using Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AccountEntityTypeConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(x => x.Id).HasName("PK_Accounts_Id");

        builder.Property(account => account.Id).HasColumnName("Id").ValueGeneratedNever().IsRequired();

        builder
            .Property(account => account.Name)
            .HasColumnName("Name")
            .HasMaxLength(AccountConstants.MaxNameLength)
            .IsRequired();

        builder.Property(account => account.Type).HasColumnName("Type").HasConversion<string>().IsRequired();

        builder.Property(account => account.Balance).HasColumnName("Balance").IsRequired();

        builder
            .HasOne(account => account.User)
            .WithMany(user => user.Accounts)
            .HasForeignKey(account => account.UserId)
            .HasConstraintName("FK_Accounts_Users_UserId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(account => account.CreatedAt).HasColumnName("CreatedAt").IsRequired();

        builder.Property(account => account.UpdatedAt).HasColumnName("UpdatedAt").IsRequired();

        builder.Property(account => account.DeletedAt).HasColumnName("DeletedAt").IsRequired(false);

        builder.HasQueryFilter(account => account.DeletedAt == null);
    }
}
