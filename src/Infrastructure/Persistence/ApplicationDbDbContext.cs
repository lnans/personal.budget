using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbDbContext(DbContextOptions<ApplicationDbDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<OperationTag> OperationTags => Set<OperationTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OperationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OperationTagEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
    }
}