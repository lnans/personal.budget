using Domain.Accounts;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.Accounts;

public class AccountCreateTests
{
    [Fact]
    public void Account_Create_WithValidParameters_ShouldCreateAccount()
    {
        // Arrange
        const string accountName = "Test Account";
        const decimal initialBalance = 100m;
        var user = UserFixture.CreateValidUser();
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var account = Account.Create(user.Id, accountName, initialBalance, createdAt);

        // Assert
        account.IsError.ShouldBeFalse();
        account.Value.UserId.ShouldBe(user.Id);
        account.Value.Name.ShouldBe(accountName);
        account.Value.Balance.ShouldBe(initialBalance);
        account.Value.Operations.ShouldBeEmpty();
        account.Value.CreatedAt.ShouldBe(createdAt);
        account.Value.UpdatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void Account_Create_WithEmptyName_ShouldReturnError()
    {
        // Arrange
        const string accountName = "";
        const decimal initialBalance = 100m;
        var user = UserFixture.CreateValidUser();
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var account = Account.Create(user.Id, accountName, initialBalance, createdAt);

        // Assert
        FixtureBase.AssertError(account, AccountErrors.AccountNameRequired);
    }

    [Fact]
    public void Account_Create_WithTooLongName_ShouldReturnError()
    {
        // Arrange
        const decimal initialBalance = 100m;
        var user = UserFixture.CreateValidUser();
        var accountName = AccountFixture.GenerateLongAccountName();
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var account = Account.Create(user.Id, accountName, initialBalance, createdAt);

        // Assert
        FixtureBase.AssertError(account, AccountErrors.AccountNameTooLong);
    }
}
