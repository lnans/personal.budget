using Infrastructure.Persistence;

namespace Api.Configurations;

/// <summary>
///     DbContext configuration, apply migration and try to seed database with default user if needed.
/// </summary>
public static class DbContextConfiguration
{
    public static async Task InitDbContext(this IApplicationBuilder app)
    {
        await using var scope = app.ApplicationServices.CreateAsyncScope();
        var dbInitializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();
        await dbInitializer.InitialiseAsync();
    }
}