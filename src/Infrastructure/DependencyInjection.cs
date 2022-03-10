using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Application;
using Infrastructure.Persistence;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbName)
    {
        return services.AddDbContext<IApplicationDbContext, ApplicationDbDbContext>(options =>
        {
            options.UseSqlite(dbName);
        });
    }
}