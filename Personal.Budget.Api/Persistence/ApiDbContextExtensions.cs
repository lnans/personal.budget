using Microsoft.EntityFrameworkCore;
using Personal.Budget.Api.Common;
using Personal.Budget.Api.Domain;
using Personal.Budget.Api.Helpers;

namespace Personal.Budget.Api.Persistence;

public static class ApiDbContextExtensions
{
    public static void InitDatabase(this IApplicationBuilder app)
    {
        var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<ApiDbContext>()!;
        dbContext.Database.Migrate();

        if (!dbContext.Users.Any())
        {
            var defaultUserSettings = scope.ServiceProvider.GetService<DefaultUserSettings>()!;
            dbContext.Users.Add(new User()
            {
                Username = defaultUserSettings.UserName,
                Hash = HashHelper.GenerateHash(defaultUserSettings.UserName, defaultUserSettings.Password)
            });
            dbContext.SaveChanges();
        }

        scope.Dispose();
    }
}