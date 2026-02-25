using Api;
using Api.Configurations;
using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

Log.Logger = new LoggerConfiguration().WithConsoleConfig().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.ConfigureLogs();

    var configuration = builder.Configuration;
    configuration.AddEnvironmentVariables();

    var services = builder.Services;
    services.AddApiServices(configuration);
    services.AddApplicationServices();
    services.AddInfrastructureServices(configuration);

    var app = builder.Build();

    // Configure forwarded headers for reverse proxy (nginx with SSL)
    app.UseForwardedHeaders(
        new ForwardedHeadersOptions
        {
            ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
        }
    );

    app.UseExceptionHandling();
    app.UseCors(config =>
    {
        config.AllowAnyHeader();
        config.AllowAnyOrigin();
        config.AllowAnyMethod();
        config.WithExposedHeaders("Content-Disposition");
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapOpenApiEndpoints();
    app.MapApiEndpoints();

    app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
        .WithName("HealthCheck");

    await app.Services.InitialiseDatabaseAsync();

    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
