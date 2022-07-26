var builder = WebApplication.CreateBuilder(args);

var dbName = builder.Configuration.GetConnectionString("Database");
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
var defaultUser = builder.Configuration.GetValue<string>("DefaultUser:Username");
var defaultPassword = builder.Configuration.GetValue<string>("DefaultUser:Password");

void JsonOptions(JsonOptions options) => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

builder.Services
    .AddApplication()
    .AddInfrastructure(dbName)
    .Configure((Action<JsonOptions>) JsonOptions)
    .AddCors()
    .AddJwtAuthentication(jwtSettings)
    .AddSwaggerGenWithSecurity()
    .AddAuthorization()
    .AddSingleton(jwtSettings)
    .AddScoped<IHttpUserContext, HttpUserContext>()
    .AddHttpContextAccessor()
    .AddEndpointsApiExplorer();

var webApp = builder.Build();
webApp
    .UseSwagger()
    .UseSwaggerUI()
    .UseCors(options =>
    {
        options.AllowAnyHeader();
        options.AllowAnyMethod();
        options.AllowAnyOrigin();
    })
    .UseAuthentication()
    .UseAuthorization()
    .UseMiddleware<ExceptionMiddleware>()
    .InitDbContext(defaultUser, defaultPassword);

webApp.MapEndPoints();
webApp.Run();