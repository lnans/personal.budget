using System.IdentityModel.Tokens.Jwt;

namespace Api.Context;

public class HttpUserContext : IHttpUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserContext(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public string GetUserId()
    {
        var claim = _httpContextAccessor?.HttpContext?
            .User
            .Claims
            .FirstOrDefault(cl => cl.Type == JwtRegisteredClaimNames.Jti);

        if (claim is not null) return claim.Value;

        throw new AuthenticationException();
    }
}