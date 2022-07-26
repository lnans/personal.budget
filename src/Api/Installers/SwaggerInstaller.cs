using System.Reflection;

namespace Api.Installers;

public static class SwaggerInstaller
{
    private static string XmlCommentsFilePath
    {
        get
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var fileName = typeof(Program).GetTypeInfo().Assembly.GetName().Name + ".xml";
            return Path.Combine(basePath, fileName);
        }
    }

    public static IServiceCollection AddSwaggerGenWithSecurity(this IServiceCollection services)
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JSON Web Token based security"
        };

        var securityReq = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        };

        var info = new OpenApiInfo
        {
            Version = "v1",
            Title = "Personal Budget",
            Description = "An API to managed your money"
        };

        return services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", info);
            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(securityReq);
            options.IncludeXmlComments(XmlCommentsFilePath);
            options.EnableAnnotations();
        });
    }
}