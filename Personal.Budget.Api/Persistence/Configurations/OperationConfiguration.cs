using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Personal.Budget.Api.Domain;

namespace Personal.Budget.Api.Persistence.Configurations;

public class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.Property(t => t.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.Amount).IsRequired();
        builder.Property(p => p.CreationDate).IsRequired();
        builder.Property(p => p.Type).HasConversion<string>().IsRequired();
        builder.Property(p => p.CreatedById).IsRequired();

        builder
            .HasOne(p => p.Account)
            .WithMany(p => p.Operations)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}