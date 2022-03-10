var builder = WebApplication.CreateBuilder(args);
var dbName = builder.Configuration.GetConnectionString("Database");
void JsonOptions(JsonOptions options) => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

// Register services
builder.Services
    .AddApplication()
    .AddInfrastructure(dbName)
    .AddSwaggerGen()
    .Configure((Action<JsonOptions>) JsonOptions)
    .AddControllers();

// Register middlewares
var api = builder.Build();
api.MapControllers();
api.UseSwagger();
api.UseSwaggerUI();
api.UseMiddleware<ExceptionMiddleware>();

// Database migration
var scope = api.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetService<ApplicationDbDbContext>();
var provider = dbContext?.Database.ProviderName;
if (provider is not null && !provider.Contains("InMemory"))
    dbContext.Database.Migrate();
scope.Dispose();

// Start
api.Run();