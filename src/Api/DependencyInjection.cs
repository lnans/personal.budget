namespace Api;

public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddCors();
    }
}
