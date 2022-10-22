global using FastEndpoints;
global using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Personal.Budget.Api.Common;
using Personal.Budget.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database")!;
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
var defaultUserSettings = builder.Configuration.GetSection("DefaultUser").Get<DefaultUserSettings>()!;

builder.Services.AddAuthenticationJWTBearer(jwtSettings.Key, jwtSettings.Issuer, jwtSettings.Audience);
builder.Services.AddDbContext<ApiDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDoc();
builder.Services.AddSingleton(_ => jwtSettings);
builder.Services.AddSingleton(_ => defaultUserSettings);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseOpenApi();
app.UseSwaggerUi3(s => s.ConfigureDefaults());
app.InitDatabase();

app.Run();