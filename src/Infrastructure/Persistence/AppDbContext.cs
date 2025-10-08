using System.Reflection;
using Application.Interfaces;
using Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

internal class AppDbContext : DbContext, IAppDbContext
{
    private readonly ILoggerFactory _loggerFactory;

    public DbSet<Account> Accounts => Set<Account>();

    public AppDbContext(DbContextOptions<AppDbContext> options, ILoggerFactory loggerFactory)
        : base(options)
    {
        _loggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLoggerFactory(_loggerFactory);
        optionsBuilder.LogTo(_ => { }, [CoreEventId.QueryExecutionPlanned, CoreEventId.ContextInitialized]);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
