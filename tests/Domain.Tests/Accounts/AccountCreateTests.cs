using Domain.Accounts;

namespace Domain.Tests.Accounts;

public class AccountCreateTests : AccountTestsBase
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateAccount()
    {
        // Arrange
        const string accountName = "Test Account";
        const decimal initialBalance = 100m;
        var createdAt = GetTestDate();

        // Act
        var account = Account.Create(accountName, initialBalance, createdAt);

        // Assert
        account.IsError.ShouldBeFalse();
        account.Value.Name.ShouldBe(accountName);
        account.Value.Balance.ShouldBe(initialBalance);
        account.Value.Operations.ShouldBeEmpty();
        account.Value.CreatedAt.ShouldBe(createdAt);
        account.Value.UpdatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnError()
    {
        // Arrange
        const string accountName = "";
        const decimal initialBalance = 100m;
        var createdAt = GetTestDate();

        // Act
        var account = Account.Create(accountName, initialBalance, createdAt);

        // Assert
        AssertError(account, AccountErrors.AccountNameRequired);
    }

    [Fact]
    public void Create_WithTooLongName_ShouldReturnError()
    {
        // Arrange
        var accountName = GenerateLongAccountName();
        const decimal initialBalance = 100m;
        var createdAt = GetTestDate();

        // Act
        var account = Account.Create(accountName, initialBalance, createdAt);

        // Assert
        AssertError(account, AccountErrors.AccountNameTooLong);
    }
}
