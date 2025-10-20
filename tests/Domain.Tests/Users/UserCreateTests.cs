using Domain.Users;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.Users;

public class UserCreateTests
{
    [Fact]
    public void User_Create_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        const string login = "testuser";
        const string passwordHash = "hashedpassword123";
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var user = User.Create(login, passwordHash, createdAt);

        // Assert
        user.IsError.ShouldBeFalse();
        user.Value.Login.ShouldBe(login);
        user.Value.PasswordHash.ShouldBe(passwordHash);
        user.Value.CreatedAt.ShouldBe(createdAt);
        user.Value.UpdatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void User_Create_WithEmptyLogin_ShouldReturnError()
    {
        // Arrange
        const string login = "";
        const string passwordHash = "hashedpassword123";
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var user = User.Create(login, passwordHash, createdAt);

        // Assert
        FixtureBase.AssertError(user, UserErrors.UserLoginRequired);
    }

    [Fact]
    public void User_Create_WithWhitespaceLogin_ShouldReturnError()
    {
        // Arrange
        const string login = "   ";
        const string passwordHash = "hashedpassword123";
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var user = User.Create(login, passwordHash, createdAt);

        // Assert
        FixtureBase.AssertError(user, UserErrors.UserLoginRequired);
    }

    [Fact]
    public void User_Create_WithNullLogin_ShouldReturnError()
    {
        // Arrange
        const string? login = null;
        const string passwordHash = "hashedpassword123";
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var user = User.Create(login!, passwordHash, createdAt);

        // Assert
        FixtureBase.AssertError(user, UserErrors.UserLoginRequired);
    }

    [Fact]
    public void User_Create_WithTooLongLogin_ShouldReturnError()
    {
        // Arrange
        var login = UserFixture.GenerateLongLogin();
        const string passwordHash = "hashedpassword123";
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var user = User.Create(login, passwordHash, createdAt);

        // Assert
        FixtureBase.AssertError(user, UserErrors.UserLoginTooLong);
    }

    [Fact]
    public void User_Create_WithEmptyPasswordHash_ShouldReturnError()
    {
        // Arrange
        const string login = "testuser";
        const string passwordHash = "";
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var user = User.Create(login, passwordHash, createdAt);

        // Assert
        FixtureBase.AssertError(user, UserErrors.UserPasswordHashRequired);
    }

    [Fact]
    public void User_Create_WithWhitespacePasswordHash_ShouldReturnError()
    {
        // Arrange
        const string login = "testuser";
        const string passwordHash = "   ";
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var user = User.Create(login, passwordHash, createdAt);

        // Assert
        FixtureBase.AssertError(user, UserErrors.UserPasswordHashRequired);
    }

    [Fact]
    public void User_Create_WithNullPasswordHash_ShouldReturnError()
    {
        // Arrange
        const string login = "testuser";
        const string? passwordHash = null;
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var user = User.Create(login, passwordHash!, createdAt);

        // Assert
        FixtureBase.AssertError(user, UserErrors.UserPasswordHashRequired);
    }

    [Fact]
    public void User_Create_WithMaxLoginLength_ShouldCreateUser()
    {
        // Arrange
        var login = FixtureBase.GenerateLongString(UserConstants.MaxLoginLength);
        const string passwordHash = "hashedpassword123";
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var user = User.Create(login, passwordHash, createdAt);

        // Assert
        user.IsError.ShouldBeFalse();
        user.Value.Login.ShouldBe(login);
    }
}
