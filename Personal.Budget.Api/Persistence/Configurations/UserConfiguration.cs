using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Personal.Budget.Api.Domain;

namespace Personal.Budget.Api.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(t => t.Username).IsUnique();

        builder.Property(t => t.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(t => t.Username).IsRequired();
        builder.Property(t => t.Hash).IsRequired();
    }
}