using Domain.Users;
using TestFixtures.Domain;

namespace Domain.Tests.Users;

public class UserGenerateRefreshTokenTests
{
    [Fact]
    public void User_GenerateRefreshToken_ShouldReturnToken_AndCallGenerator()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var authTokenGenerator = Substitute.For<IAuthTokenGenerator>();
        const string expectedRefreshToken = "refresh-token-123";

        authTokenGenerator.GenerateRefreshToken(user.Id).Returns(expectedRefreshToken);

        // Act
        var result = user.GenerateRefreshToken(authTokenGenerator);

        // Assert
        result.ShouldBe(expectedRefreshToken);
        authTokenGenerator.Received(1).GenerateRefreshToken(user.Id);
    }
}
