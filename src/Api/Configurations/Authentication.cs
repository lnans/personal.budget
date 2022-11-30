using System.Security.Authentication;
using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api.Configurations;

/// <summary>
///     Class used to read auth settings from appsettings.json
/// </summary>
internal sealed class AuthSettings
{
    public string Authority { get; init; } = null!;
    public string Audience { get; init; } = null!;
}

/// <summary>
///     Authentication configuration for the API using Auth0
/// </summary>
internal static class AuthenticationConfiguration
{
    /// <summary>
    ///     Configure and enable jwt bearer authentication
    ///     and register <see cref="AuthSettings" /> in the service provider
    /// </summary>
    /// services.AddAuth0WebAppAuthentication()
    /// <param name="services">The web api services from builder</param>
    /// <param name="authSettings">The authentication settings</param>
    public static void AddAuthenticationAuth0(this IServiceCollection services, AuthSettings? authSettings)
    {
        services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = authSettings?.Authority;
                options.Audience = authSettings?.Audience;
            });
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
    }
}

public sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetAuthenticatedUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext!;

        var claim = httpContext.User.Claims.FirstOrDefault(cl => cl.Type == ClaimTypes.NameIdentifier);
        if (claim is not null) return claim.Value;

        throw new AuthenticationException();
    }
}