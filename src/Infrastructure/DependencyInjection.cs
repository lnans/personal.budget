using Application.Interfaces;
using Ardalis.GuardClauses;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("Database");
        Guard.Against.NullOrEmpty(connectionString, nameof(connectionString));

        services.ConfigurePersistence(connectionString);
    }

    private static void ConfigurePersistence(
        this IServiceCollection services,
        string connectionString
    ) =>
        services.AddDbContext<IAppDbContext, AppDbContext>(config =>
            config.UseNpgsql(connectionString)
        );
}
