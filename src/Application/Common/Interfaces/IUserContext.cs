using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces;

public interface IUserContext
{
    string GetUserId();
}

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

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