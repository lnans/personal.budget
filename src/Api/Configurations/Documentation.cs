using System.Net;
using System.Reflection;
using Application.Common.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Configurations;

/// <summary>
///     A set of swagger documentation extensions
/// </summary>
internal static class DocumentationExtensions
{
    /// <summary>
    ///     Configure Swagger documentation
    /// </summary>
    /// <param name="services">The web api services from builder</param>
    public static IServiceCollection AddSwaggerDoc(this IServiceCollection services)
    {
        // Assembly infos
        var assemblyName = typeof(Program).Assembly.GetName();
        var assemblyVersion = assemblyName.Version!;
        var version = $"v{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Revision}";

        // Xml documentation
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var fileName = typeof(Program).GetTypeInfo().Assembly.GetName().Name + ".xml";
        var xmlPath = Path.Combine(basePath, fileName);

        var info = new OpenApiInfo
        {
            Version = version,
            Title = assemblyName.Name,
            Description = "A simple API to manage your finance"
        };

        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
            Description = "Enter a JWT token to authorize the requests..."
        };

        var securityRequirement = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        };

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("default", info);
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
            options.AddSecurityRequirement(securityRequirement);
            options.IncludeXmlComments(xmlPath);
            options.EnableAnnotations();
        });

        return services;
    }

    /// <summary>
    ///     Register Swagger middleware
    /// </summary>
    /// <param name="app">The application builder</param>
    public static IApplicationBuilder UseSwaggerDoc(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/default/swagger.json", "default"));
        return app;
    }

    /// <summary>
    ///     Add summary and a description definition to an API endpoint on swagger
    /// </summary>
    /// <param name="builder">Endpoint builder</param>
    /// <param name="summary">Summary</param>
    /// <param name="description">Description</param>
    /// <returns>Endpoint builder</returns>
    public static RouteHandlerBuilder Summary(this RouteHandlerBuilder builder, string summary, string description) =>
        builder.WithMetadata(new SwaggerOperationAttribute(summary, description));

    /// <summary>
    ///     Describe a success http request on swagger
    /// </summary>
    /// <param name="builder">Endpoint builder</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="description">Description</param>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <returns>Endpoint builder</returns>
    public static RouteHandlerBuilder ProduceResponse<TResponse>(this RouteHandlerBuilder builder, HttpStatusCode statusCode, string description) =>
        builder.WithMetadata(new SwaggerResponseAttribute((int)statusCode, description, typeof(TResponse)));

    /// <summary>
    ///     Describe a success http request on swagger
    /// </summary>
    /// <param name="builder">Endpoint builder</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="description">Description</param>
    /// <returns>Endpoint builder</returns>
    public static RouteHandlerBuilder ProduceResponse(this RouteHandlerBuilder builder, HttpStatusCode statusCode, string description) =>
        builder.WithMetadata(new SwaggerResponseAttribute((int)statusCode, description));

    /// <summary>
    ///     Describe an error response on swagger
    /// </summary>
    /// <param name="builder">Endpoint builder</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="description">Description</param>
    /// <returns>Endpoint builder</returns>
    public static RouteHandlerBuilder ProduceError(this RouteHandlerBuilder builder, HttpStatusCode statusCode, string description) =>
        builder.WithMetadata(new SwaggerResponseAttribute((int)statusCode, description, typeof(ErrorResponse)));
}