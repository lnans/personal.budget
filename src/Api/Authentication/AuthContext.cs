using System.Security.Claims;
using Application.Interfaces;

namespace Api.Authentication;

internal sealed class AuthContext : IAuthContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid CurrentUserId
    {
        get
        {
            var httpContext =
                _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HTTP context is not available");

            var userIdClaim =
                httpContext.User.FindFirst(ClaimTypes.NameIdentifier) ?? httpContext.User.FindFirst("sub");

            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new InvalidOperationException("User ID claim is not available or invalid");
            }

            return userId;
        }
    }
}
