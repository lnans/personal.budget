using Domain.Accounts;

namespace Domain.Tests.Accounts;

public class AccountRenameTests : AccountTestsBase
{
    [Fact]
    public void Rename_WithValidParameters_ShouldRenameAccount()
    {
        // Arrange
        var account = CreateValidAccount();
        var updatedAt = GetTestDate(1);
        const string newName = "Renamed Account";

        // Act
        var result = account.Rename(newName, updatedAt);

        // Assert
        AssertSuccess(result);
        account.Name.ShouldBe(newName);
        account.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void Rename_WithEmptyName_ShouldReturnError()
    {
        // Arrange
        var account = CreateValidAccount();
        var updatedAt = GetTestDate(1);

        // Act
        var result = account.Rename("", updatedAt);

        // Assert
        AssertError(result, AccountErrors.AccountNameRequired);
    }

    [Fact]
    public void Rename_WithTooLongName_ShouldReturnError()
    {
        // Arrange
        var account = CreateValidAccount();
        var updatedAt = GetTestDate(1);
        var newName = GenerateLongAccountName();

        // Act
        var result = account.Rename(newName, updatedAt);

        // Assert
        AssertError(result, AccountErrors.AccountNameTooLong);
    }
}
