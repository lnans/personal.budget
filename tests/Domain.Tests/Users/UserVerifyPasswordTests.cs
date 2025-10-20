using Domain.Users;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.Users;

public class UserVerifyPasswordTests
{
    [Fact]
    public void User_VerifyPassword_WithValidPassword_ShouldReturnSuccess()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        const string passwordToVerify = "correctpassword";

        passwordHasher.Verify(passwordToVerify, user.PasswordHash).Returns(true);

        // Act
        var result = user.VerifyPassword(passwordToVerify, passwordHasher);

        // Assert
        FixtureBase.AssertSuccess(result);
        passwordHasher.Received(1).Verify(passwordToVerify, user.PasswordHash);
    }

    [Fact]
    public void User_VerifyPassword_WithInvalidPassword_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        const string passwordToVerify = "wrongpassword";

        passwordHasher.Verify(passwordToVerify, user.PasswordHash).Returns(false);

        // Act
        var result = user.VerifyPassword(passwordToVerify, passwordHasher);

        // Assert
        FixtureBase.AssertError(result, UserErrors.UserInvalidCredentials);
        passwordHasher.Received(1).Verify(passwordToVerify, user.PasswordHash);
    }

    [Fact]
    public void User_VerifyPassword_WithEmptyPassword_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        const string passwordToVerify = "";

        // Act
        var result = user.VerifyPassword(passwordToVerify, passwordHasher);

        // Assert
        FixtureBase.AssertError(result, UserErrors.UserInvalidCredentials);
        passwordHasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void User_VerifyPassword_WithNullPassword_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        const string? passwordToVerify = null;

        // Act
        var result = user.VerifyPassword(passwordToVerify!, passwordHasher);

        // Assert
        FixtureBase.AssertError(result, UserErrors.UserInvalidCredentials);
        passwordHasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }
}
