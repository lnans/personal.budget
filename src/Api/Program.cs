using Api;
using Api.Configurations;
using Application;
using Infrastructure;
using Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

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
