namespace Infrastructure.Persistence.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(p => p.Username).IsRequired();
        builder.Property(p => p.Hash).IsRequired();
    }
}