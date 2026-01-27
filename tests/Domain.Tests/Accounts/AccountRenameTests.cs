using Domain.Accounts;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.Accounts;

public class AccountRenameTests
{
    [Fact]
    public void Account_Rename_WithValidParameters_ShouldRenameAccount()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);
        const string newName = "Renamed Account";
        const string newBank = "Updated Bank";

        // Act
        var result = account.Rename(newName, newBank, updatedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        account.Name.ShouldBe(newName);
        account.Bank.ShouldBe(newBank);
        account.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void Account_Rename_WithEmptyName_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);

        // Act
        var result = account.Rename("", account.Bank, updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountErrors.AccountNameRequired);
    }

    [Fact]
    public void Account_Rename_WithTooLongName_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);
        var newName = AccountFixture.GenerateLongAccountName();

        // Act
        var result = account.Rename(newName, account.Bank, updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountErrors.AccountNameTooLong);
    }

    [Fact]
    public void Account_Rename_WithEmptyBank_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);

        // Act
        var result = account.Rename("Renamed Account", "", updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountErrors.AccountBankRequired);
    }

    [Fact]
    public void Account_Rename_WithTooLongBank_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);
        var newBank = AccountFixture.GenerateLongAccountBank();

        // Act
        var result = account.Rename("Renamed Account", newBank, updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountErrors.AccountBankTooLong);
    }
}
