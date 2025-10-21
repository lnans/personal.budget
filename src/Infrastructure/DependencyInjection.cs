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
        services.ConfigureAuthentication(configuration);
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
        services.AddSingleton(TimeProvider.System);
    }

    private static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authOptions = configuration.GetAuthTokenOptions();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IAuthTokenGenerator, AuthTokenGenerator>();
        services.AddSingleton(authOptions);
    }
}
