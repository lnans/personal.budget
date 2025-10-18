namespace Api.Configurations;

public interface IEndPoints
{
    void MapEndpoints(WebApplication app);
}

public static class EndpointsExtensions
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        var endPointsInterface = typeof(IEndPoints);
        var endPointsClasses = typeof(Program)
            .Assembly.GetTypes()
            .Where(type =>
                type.IsClass && endPointsInterface.IsAssignableFrom(type)
            );

        foreach (var endPointsClass in endPointsClasses)
        {
            var instance = (IEndPoints)
                Activator.CreateInstance(endPointsClass)!;
            instance.MapEndpoints(app);
        }
    }
}
