using Api;
using Api.Configurations;
using Application;
using Infrastructure;
using Infrastructure.Persistence;
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

    services.AddApiServices();
    services.AddApplicationServices(configuration);
    services.AddInfrastructureServices(configuration);

    var app = builder.Build();

    app.UseDeveloperExceptionPage();
    app.UseCors(config =>
    {
        config.AllowAnyHeader();
        config.AllowAnyOrigin();
        config.AllowAnyMethod();
        config.WithExposedHeaders("Content-Disposition");
    });

    app.MapOpenApiEndpoints();
    app.MapApiEndpoints();

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
