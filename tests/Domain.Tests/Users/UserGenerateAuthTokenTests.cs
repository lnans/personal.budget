using Domain.Users;
using TestFixtures.Domain;

namespace Domain.Tests.Users;

public class UserGenerateAuthTokenTests
{
    [Fact]
    public void User_GenerateAuthToken_ShouldReturnToken_AndCallGenerator()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var authTokenGenerator = Substitute.For<IAuthTokenGenerator>();
        const string expectedToken = "token-123";

        authTokenGenerator.GenerateToken(user.Id, user.Login).Returns(expectedToken);

        // Act
        var result = user.GenerateAuthToken(authTokenGenerator);

        // Assert
        result.ShouldBe(expectedToken);
        authTokenGenerator.Received(1).GenerateToken(user.Id, user.Login);
    }
}
