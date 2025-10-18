using Domain.AccountOperations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AccountOperationEntityTypeConfiguration : IEntityTypeConfiguration<AccountOperation>
{
    public void Configure(EntityTypeBuilder<AccountOperation> builder)
    {
        builder.ToTable("AccountOperations");

        builder.HasKey(accountOperation => accountOperation.Id).HasName("PK_AccountOperations_Id");

        builder
            .Property(accountOperation => accountOperation.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .HasOne(accountOperation => accountOperation.Account)
            .WithMany(account => account.Operations)
            .HasForeignKey(accountOperation => accountOperation.AccountId)
            .HasConstraintName("FK_AccountOperations_Accounts_AccountId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(accountOperation => accountOperation.Description)
            .HasColumnName("Description")
            .HasMaxLength(AccountOperationConstants.MaxDescriptionLength)
            .IsRequired();

        builder.Property(accountOperation => accountOperation.Amount).HasColumnName("Amount").IsRequired();

        builder
            .Property(accountOperation => accountOperation.PreviousBalance)
            .HasColumnName("PreviousBalance")
            .IsRequired();

        builder.Property(accountOperation => accountOperation.NextBalance).HasColumnName("NextBalance").IsRequired();

        builder.Property(accountOperation => accountOperation.CreatedAt).HasColumnName("CreatedAt").IsRequired();

        builder.Property(accountOperation => accountOperation.UpdatedAt).HasColumnName("UpdatedAt").IsRequired();
    }
}
