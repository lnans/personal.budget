using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id).HasName("PK_Users_Id");

        builder.Property(user => user.Id).HasColumnName("Id").ValueGeneratedNever().IsRequired();

        builder
            .Property(user => user.Login)
            .HasColumnName("Login")
            .HasMaxLength(UserConstants.MaxLoginLength)
            .IsRequired();

        builder.Property(user => user.PasswordHash).HasColumnName("PasswordHash").IsRequired();

        builder.Property(user => user.CreatedAt).HasColumnName("CreatedAt").IsRequired();

        builder.Property(user => user.UpdatedAt).HasColumnName("UpdatedAt").IsRequired();
    }
}
