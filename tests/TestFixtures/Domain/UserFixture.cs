using Domain.Users;

namespace TestFixtures.Domain;

public static class UserFixture
{
    public static User CreateValidUser(
        string login = "testuser",
        string passwordHash = "hashedpassword123",
        DateTimeOffset? createdAt = null
    ) => User.Create(login, passwordHash, createdAt ?? DateTimeOffset.UtcNow).Value;

    public static string GenerateLongLogin() => FixtureBase.GenerateLongString(UserConstants.MaxLoginLength + 1);
}
