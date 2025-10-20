using Application.Interfaces;
using Ardalis.GuardClauses;
using Domain.Users;
using Infrastructure.Authentication;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigurePersistence(configuration);
        services.ConfigureAuthentication();
    }

    private static void ConfigurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        Guard.Against.NullOrEmpty(
            connectionString,
            nameof(connectionString),
            "Connection string 'Database' is not found."
        );

        services.AddDbContext<IAppDbContext, AppDbContext>(config => config.UseNpgsql(connectionString));
        services.AddScoped<AppDbContextInitializer>();
    }

    private static void ConfigureAuthentication(this IServiceCollection services) =>
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
}
