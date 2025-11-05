using System.Text;
using Api.Authentication;
using Api.Errors;
using Application.Interfaces;
using Domain.Users;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace Api;

public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureOpenApi();
        services.ConfigureAuthentication(configuration);

        services.AddCors();
        services.AddDataProtection().SetApplicationName("Budget.Api");
    }

    private static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authOptions = configuration.GetAuthTokenOptions();
        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthTokenGenerator, AuthTokenGenerator>();
        services.AddScoped<IAuthContext, AuthContext>();
        services.AddSingleton(authOptions);
        services.AddSingleton(TimeProvider.System);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authOptions.Issuer,
                    ValidAudience = authOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero,
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // Prevent default behavior (adding WWW-Authenticate header and empty body)
                        context.HandleResponse();

                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = Problems.ProblemContentType;

                        var problem = Problems.Unauthorized(context.HttpContext);
                        await context.Response.WriteAsJsonAsync(problem);
                    },
                    OnForbidden = async context =>
                    {
                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = Problems.ProblemContentType;

                        var problem = Problems.Forbidden(context.HttpContext);
                        await context.Response.WriteAsJsonAsync(problem);
                    },
                };
            });
        services.AddAuthorization();
    }

    private static void ConfigureOpenApi(this IServiceCollection services) =>
        services.AddOpenApi(config =>
        {
            config.AddScalarTransformers();
            config.AddDocumentTransformer(
                (document, _, _) =>
                {
                    var httpsUrls = document
                        .Servers.Where(s => s.Url.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                        .Select(s => s.Url)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var serversToAdd = new List<OpenApiServer>();

                    foreach (
                        var httpsUrl in from server in document.Servers.ToList()
                        where server.Url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                        select server.Url.Replace("http:", "https:") into httpsUrl
                        where !httpsUrls.Contains(httpsUrl)
                        select httpsUrl
                    )
                    {
                        serversToAdd.Add(new OpenApiServer { Url = httpsUrl });
                        httpsUrls.Add(httpsUrl);
                    }

                    foreach (var httpsServer in serversToAdd)
                    {
                        document.Servers.Add(httpsServer);
                    }

                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Enter your JWT token in the format: your-token-here",
                    };

                    document.SecurityRequirements = new List<OpenApiSecurityRequirement>
                    {
                        new()
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer",
                                    },
                                },
                                Array.Empty<string>()
                            },
                        },
                    };
                    return Task.CompletedTask;
                }
            );
        });
}
