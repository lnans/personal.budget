namespace Api.Configurations;

public interface IEndpoints
{
    void MapEndpoints(WebApplication app);
}

public static class EndpointsExtensions
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        var endPointsInterface = typeof(IEndpoints);
        var endPointsClasses = typeof(Program)
            .Assembly.GetTypes()
            .Where(type => type.IsClass && endPointsInterface.IsAssignableFrom(type));

        foreach (var endPointsClass in endPointsClasses)
        {
            var instance = (IEndpoints)Activator.CreateInstance(endPointsClass)!;
            instance.MapEndpoints(app);
        }
    }
}
