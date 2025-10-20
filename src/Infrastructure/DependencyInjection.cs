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
        var authOptions = configuration.GetSection("Auth").Get<AuthTokenOptions>();
        Guard.Against.Null(authOptions, nameof(authOptions), "'Auth' section is not found in the configuration.");
        Guard.Against.NullOrEmpty(
            authOptions.SecretKey,
            nameof(authOptions.SecretKey),
            "'Auth:SecretKey' is not found in the configuration."
        );
        Guard.Against.NullOrEmpty(
            authOptions.Issuer,
            nameof(authOptions.Issuer),
            "'Auth:Issuer' is not found in the configuration."
        );
        Guard.Against.NullOrEmpty(
            authOptions.Audience,
            nameof(authOptions.Audience),
            "'Auth:Audience' is not found in the configuration."
        );
        Guard.Against.Zero(
            authOptions.ExpirationMinutes,
            nameof(authOptions.ExpirationMinutes),
            "'Auth:ExpirationMinutes' is not found in the configuration."
        );
        Guard.Against.StringTooShort(
            authOptions.SecretKey,
            32,
            nameof(authOptions.SecretKey),
            "'Auth:SecretKey' must be at least 32 characters long."
        );

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IAuthTokenGenerator, AuthTokenGenerator>();
        services.AddSingleton(authOptions);
    }
}
