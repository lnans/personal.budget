using Microsoft.OpenApi;
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

    public static void ConfigureOpenApi(this IServiceCollection services) =>
        services.AddOpenApi(config =>
        {
            config.AddScalarTransformers();
            config.AddDocumentTransformer(
                (document, _, _) =>
                {
                    document.Info.Title = "Budget.Api";
                    if (document.Servers != null && document.Servers.Any())
                    {
                        var httpsUrls = document
                            .Servers.Where(s =>
                                s.Url != null && s.Url.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
                            )
                            .Select(s => s.Url)
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        var serversToAdd = new List<OpenApiServer>();

                        foreach (
                            var httpsUrl in from server in document.Servers.ToList()
                            where
                                server.Url != null && server.Url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                            select server.Url!.Replace("http:", "https:") into httpsUrl
                            where !httpsUrls.Contains(httpsUrl)
                            select httpsUrl
                        )
                        {
                            serversToAdd.Add(new OpenApiServer { Url = httpsUrl });
                            httpsUrls.Add(httpsUrl);
                        }

                        foreach (var httpsServer in serversToAdd)
                        {
                            document.Servers.Add(httpsServer);
                        }
                    }

                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Enter your JWT token in the format: your-token-here",
                    };

                    document.Security = new List<OpenApiSecurityRequirement>
                    {
                        new() { { new OpenApiSecuritySchemeReference("Bearer"), [] } },
                    };
                    return Task.CompletedTask;
                }
            );
        });
}
