using Swashbuckle.AspNetCore.Annotations;

namespace Api.Installers;

public interface IEndPoints
{
    void Register(WebApplication app);
}

public static class EndPointInstaller
{
    public static void MapEndPoints(this WebApplication app)
    {
        var endPointsInterface = typeof(IEndPoints);
        var endPointsClasses = typeof(Program)
            .Assembly
            .GetTypes()
            .Where(type => type.IsClass && endPointsInterface.IsAssignableFrom(type));

        foreach (var endPointsClass in endPointsClasses)
        {
            var instance = (IEndPoints) Activator.CreateInstance(endPointsClass);
            instance!.Register(app);
        }
    }

    public static RouteHandlerBuilder Summary(this RouteHandlerBuilder builder, string summary, string description) =>
        builder.WithMetadata(new SwaggerOperationAttribute(summary, description));

    public static RouteHandlerBuilder Response<TResponse>(this RouteHandlerBuilder builder, int statusCode, string description) =>
        builder.WithMetadata(new SwaggerResponseAttribute(statusCode, description, typeof(TResponse)));

    public static RouteHandlerBuilder Response(this RouteHandlerBuilder builder, int statusCode, string description) =>
        builder.WithMetadata(new SwaggerResponseAttribute(statusCode, description));
}