using Domain.Accounts;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.Accounts;

public class AccountDeleteTests
{
    [Fact]
    public void Account_Delete_WithValidTimestamp_ShouldMarkAccountAsDeleted()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var deletedAt = FixtureBase.GetTestDate(1);

        // Act
        var result = account.Delete(deletedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        account.DeletedAt.ShouldNotBeNull();
        account.DeletedAt.ShouldBe(deletedAt);
        account.UpdatedAt.ShouldBe(deletedAt);
    }

    [Fact]
    public void Account_Delete_WhenAlreadyDeleted_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var firstDeleteAt = FixtureBase.GetTestDate(1);
        account.Delete(firstDeleteAt);

        // Act
        var secondDeleteAt = FixtureBase.GetTestDate(2);
        var result = account.Delete(secondDeleteAt);

        // Assert
        FixtureBase.AssertError(result, AccountErrors.AccountAlreadyDeleted);
        account.DeletedAt.ShouldBe(firstDeleteAt); // Should not change
    }
}
