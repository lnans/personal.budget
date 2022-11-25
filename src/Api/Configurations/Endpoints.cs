namespace Api.Configurations;

/// <summary>
///     Interface used by all class that declare endpoints
/// </summary>
public interface IEndPoints
{
    void Register(WebApplication app);
}

/// <summary>
///     Endpoints discovery on API startup
/// </summary>
public static class EndpointsExtensions
{
    public static void UseEndpoints(this WebApplication app)
    {
        var endPointsInterface = typeof(IEndPoints);
        var endPointsClasses = typeof(Program)
            .Assembly
            .GetTypes()
            .Where(type => type.IsClass && endPointsInterface.IsAssignableFrom(type));

        foreach (var endPointsClass in endPointsClasses)
        {
            var instance = (IEndPoints)Activator.CreateInstance(endPointsClass)!;
            instance.Register(app);
        }
    }
}