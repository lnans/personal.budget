using Microsoft.EntityFrameworkCore;
using Personal.Budget.Api.Domain;
using Personal.Budget.Api.Persistence.Configurations;

namespace Personal.Budget.Api.Persistence;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<OperationTag> OperationTags => Set<OperationTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new OperationConfiguration());
        modelBuilder.ApplyConfiguration(new OperationTagConfiguration());
    }
}