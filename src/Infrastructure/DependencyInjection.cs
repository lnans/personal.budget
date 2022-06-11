using Application.Common.Helpers;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbName)
    {
        return services.AddDbContext<IApplicationDbContext, ApplicationDbDbContext>(options => { options.UseSqlite(dbName); });
    }

    public static IApplicationBuilder InitDbContext(this IApplicationBuilder app, string defaultUser, string defaultPassword)
    {
        // Database migration
        var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<ApplicationDbDbContext>();
        var provider = dbContext?.Database.ProviderName;
        if (provider is not null && !provider.Contains("InMemory"))
            dbContext.Database.Migrate();

        // Default User
        if (dbContext != null && !dbContext.Users.Any())
        {
            var id = Guid.NewGuid().ToString();
            dbContext.Users.Add(new User
            {
                Id = id,
                Username = defaultUser,
                Hash = HashHelper.GenerateHash(id, defaultPassword)
            });
            dbContext.SaveChanges();
        }

        scope.Dispose();

        return app;
    }
}