using Scalar.AspNetCore;

namespace Api.Configurations;

public static class OpenApiConfiguration
{
    public static void MapOpenApiEndpoints(this WebApplication app)
    {
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference("/docs").AllowAnonymous();
    }
}
