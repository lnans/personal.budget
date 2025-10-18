using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services, IConfigurationManager configuration) =>
        services.AddMediatR(config =>
        {
            config.LicenseKey = configuration["MediatrLicenceKey"];
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
}
