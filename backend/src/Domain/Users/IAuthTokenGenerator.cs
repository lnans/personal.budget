using ErrorOr;

namespace Domain.Users;

public interface IAuthTokenGenerator
{
    string GenerateToken(Guid userId, string userLogin);
    string GenerateRefreshToken(Guid userId);
    ErrorOr<Guid> ValidateRefreshToken(string refreshToken);
}
