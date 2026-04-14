using Api.Configurations;
using Microsoft.AspNetCore.DataProtection;

namespace Api;

public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureOpenApi();
        services.ConfigureAuthentication(configuration);

        services.AddCors();
        services.AddDataProtection().SetApplicationName("Budget.Api");
    }
}
