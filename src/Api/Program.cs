using Api;
using Api.Configurations;
using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    var services = builder.Services;

    configuration.AddEnvironmentVariables();

    builder.Host.UseSerilog(
        (ctx, sv, config) => config.ReadFrom.Configuration(ctx.Configuration).ReadFrom.Services(sv)
    );

    services.AddApiServices(configuration);
    services.AddApplicationServices(configuration);
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
        .WithName("HealthCheck")
        .WithOpenApi();

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
