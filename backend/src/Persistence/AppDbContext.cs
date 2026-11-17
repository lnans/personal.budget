using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Personal.Budget.App.Persistence.Entities;

namespace Personal.Budget.App.Persistence;

internal class AppDbContext : DbContext
{
    private readonly ILoggerFactory _loggerFactory;

    public DbSet<Tag> Tags => Set<Tag>();

    public AppDbContext(DbContextOptions<AppDbContext> options, ILoggerFactory loggerFactory)
        : base(options)
    {
        _loggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseLoggerFactory(_loggerFactory);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
