using System.Net.Mime;
using System.Text;
using Api.Authentication;
using Api.Extensions;
using Application.Interfaces;
using Domain.Users;
using ErrorOr;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api.Configurations;

public static class AuthenticationConfiguration
{
    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
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
                        context.Response.ContentType = MediaTypeNames.Application.ProblemJson;

                        var error = Error.Unauthorized(
                            "Authentication.Unauthorized",
                            "Unauthorized access. Please provide a valid JWT token."
                        );
                        var problem = error.ToProblem(context.HttpContext);
                        await context.Response.WriteAsJsonAsync(problem);
                    },
                    OnForbidden = async context =>
                    {
                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = MediaTypeNames.Application.ProblemJson;

                        var error = Error.Forbidden(
                            "Authorization.Forbidden",
                            "Forbidden access. You do not have permission to access this resource."
                        );
                        var problem = error.ToProblem(context.HttpContext);
                        await context.Response.WriteAsJsonAsync(problem);
                    },
                };
            });
        services.AddAuthorization();
    }
}
