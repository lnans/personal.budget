using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AppDbContextInitializer> _logger;

    public AppDbContextInitializer(
        IConfiguration configuration,
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider,
        ILogger<AppDbContextInitializer> logger,
        AppDbContext dbContext
    )
    {
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
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
                await SeedAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    private async Task SeedAsync()
    {
        var hasUsers = await _dbContext.Users.AnyAsync();
        if (hasUsers)
        {
            return;
        }

        var defaultUserLogin = _configuration["BUDGET_USER"];
        var defaultUserPassword = _configuration["BUDGET_PASSWORD"];
        if (string.IsNullOrWhiteSpace(defaultUserLogin) || string.IsNullOrWhiteSpace(defaultUserPassword))
        {
            _logger.LogWarning(
                "Default user email or password is not set in configuration. Skipping seeding default user."
            );
            return;
        }

        var hashedPassword = _passwordHasher.Hash(defaultUserPassword);
        var now = _timeProvider.GetUtcNow();
        var user = User.Create(defaultUserLogin, hashedPassword, now);

        if (user.IsError)
        {
            _logger.LogError("Failed to create default user: {Errors}", string.Join(", ", user.Errors));
            return;
        }

        await _dbContext.Users.AddAsync(user.Value);
        await _dbContext.SaveChangesAsync();
    }
}
