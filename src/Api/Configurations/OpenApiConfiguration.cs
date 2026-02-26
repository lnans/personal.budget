using Scalar.AspNetCore;

namespace Api.Configurations;

public static class OpenApiConfiguration
{
    public static void MapOpenApiEndpoints(this WebApplication app)
    {
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference(
                "/docs",
                config =>
                {
                    config.Title = "Budget.Api";
                    config.Authentication = new ScalarAuthenticationOptions
                    {
                        PreferredSecuritySchemes = new List<string> { "Bearer" },
                    };
                }
            )
            .AllowAnonymous();
    }
}
