using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
