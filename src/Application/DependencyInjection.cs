namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
            .AddMediatR(typeof(DependencyInjection))
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(typeof(DependencyInjection).Assembly))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
}