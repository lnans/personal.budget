using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString) =>
        services
            .AddDbContext<IApplicationDbContext, ApplicationDbContext>(options => options.UseNpgsql(connectionString))
            .AddScoped<ApplicationDbContextInitializer>();
}