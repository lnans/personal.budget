namespace Domain.Users;

public interface IAuthTokenGenerator
{
    string GenerateToken(Guid userId, string userLogin);
    string GenerateRefreshToken(Guid userId);
    Guid? ValidateRefreshToken(string refreshToken);
}
