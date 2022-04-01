using Microsoft.OpenApi.Models;

namespace Api.Installers;

public static class SwaggerInstaller
{
    public static IServiceCollection AddSwaggerGenWithSecurity(this IServiceCollection services)
    {
        var securityScheme = new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JSON Web Token based security",
        };
        
        var securityReq = new OpenApiSecurityRequirement()
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
                new string[] {}
            }
        };
        
        var info = new OpenApiInfo()
        {
            Version = "v1",
            Title = "Personal Budget",
            Description = "An API to managed your money",
        };

        return services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", info);
            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(securityReq);
        });
    }
}