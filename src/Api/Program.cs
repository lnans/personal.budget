using Api.Configurations;
using Api.Middlewares;
using Application;
using Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

try
{
    builder.Configuration.AddEnvironmentVariables();

    var connectionString = builder.Configuration.GetConnectionString("Database")!;
    var authSettings = builder.Configuration.GetSection("Auth").Get<AuthSettings>()!;

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(connectionString);
    builder.Services.AddAuthenticationAuth0(authSettings);
    builder.Services.AddSwaggerDoc();

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSwaggerDoc();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseEndpoints();

    await app.InitDbContext();

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