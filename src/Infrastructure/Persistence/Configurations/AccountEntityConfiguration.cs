namespace Infrastructure.Persistence.Configurations;

public class AccountEntityConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.Balance).IsRequired();
        builder.Property(p => p.Type).HasConversion<string>().IsRequired();
        builder.Property(p => p.CreationDate).IsRequired();

        builder.HasIndex(p => p.Name).IsUnique();

        builder
            .HasOne(p => p.Owner)
            .WithMany(p => p.Accounts)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}