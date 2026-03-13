using Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TagEntityTypeConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(x => x.Id).HasName("PK_Tags_Id");

        builder.Property(tag => tag.Id).HasColumnName("Id").ValueGeneratedNever().IsRequired();

        builder.Property(tag => tag.Name).HasColumnName("Name").HasMaxLength(TagConstants.MaxNameLength).IsRequired();

        builder.Property(tag => tag.Color).HasColumnName("Color").HasMaxLength(TagConstants.ColorLength).IsRequired();

        builder
            .HasOne(tag => tag.User)
            .WithMany(user => user.Tags)
            .HasForeignKey(tag => tag.UserId)
            .HasConstraintName("FK_Tags_Users_UserId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(tag => tag.CreatedAt).HasColumnName("CreatedAt").IsRequired();

        builder.Property(tag => tag.UpdatedAt).HasColumnName("UpdatedAt").IsRequired();

        builder.Property(tag => tag.DeletedAt).HasColumnName("DeletedAt").IsRequired(false);

        builder.HasQueryFilter(tag => tag.DeletedAt == null);
    }
}
