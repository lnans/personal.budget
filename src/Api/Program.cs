using Application.Common.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var dbName = builder.Configuration.GetConnectionString("Database");
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
var defaultUser = builder.Configuration.GetValue<string>("DefaultUser:Username");
var defaultPassword = builder.Configuration.GetValue<string>("DefaultUser:Password");
void JsonOptions(JsonOptions options) => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

// Register services
builder.Services
    .AddApplication()
    .AddInfrastructure(dbName)
    .Configure((Action<JsonOptions>) JsonOptions)
    .AddJwtAuthentication(jwtSettings)
    .AddSwaggerGenWithSecurity()
    .AddAuthorization()
    .AddSingleton(jwtSettings)
    .AddScoped<IUserContext, UserContext>()
    .AddHttpContextAccessor()
    .AddEndpointsApiExplorer()
    .AddControllers();

// Register middlewares
var api = builder.Build();

if (api.Environment.IsDevelopment())
{
    api.UseSwagger()
        .UseSwaggerUI();
}

api.UseAuthentication()
    .UseAuthorization()
    .UseMiddleware<ExceptionMiddleware>();

api.MapControllers();

// Database migration
var scope = api.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetService<ApplicationDbDbContext>();
var provider = dbContext?.Database.ProviderName;
if (provider is not null && !provider.Contains("InMemory"))
    dbContext.Database.Migrate();

// Default User
if (dbContext != null && !dbContext.Users.Any())
{
    var id = Guid.NewGuid().ToString();
    dbContext.Users.Add(new User
    {
        Id = id,
        Username = defaultUser,
        Hash = HashHelper.GenerateHash(id, defaultPassword)
    });
    dbContext.SaveChanges();
}

scope.Dispose();

// Start
api.Run();