using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static class InitializerExtensions
{
    public static async Task InitialiseDatabaseAsync(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();

        var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();

        await initializer.InitialiseAsync();
    }
}

internal class AppDbContextInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AppDbContextInitializer> _logger;

    public AppDbContextInitializer(ILogger<AppDbContextInitializer> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }
}
