namespace Domain.Users;

public interface IAuthTokenGenerator
{
    string GenerateToken(Guid userId, string userLogin);
}
