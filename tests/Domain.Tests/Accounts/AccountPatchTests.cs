using Domain.Accounts;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.Accounts;

public class AccountPatchTests
{
    [Fact]
    public void Account_Patch_WithValidParameters_ShouldPatchAccount()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);
        const string newName = "Patched Account";
        const string newBank = "Updated Bank";

        // Act
        var result = account.Patch(newName, newBank, null, updatedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        account.Name.ShouldBe(newName);
        account.Bank.ShouldBe(newBank);
        account.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void Account_Patch_WithEmptyName_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);

        // Act
        var result = account.Patch("", account.Bank, null, updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountErrors.AccountNameRequired);
    }

    [Fact]
    public void Account_Patch_WithTooLongName_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);
        var newName = AccountFixture.GenerateLongAccountName();

        // Act
        var result = account.Patch(newName, account.Bank, null, updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountErrors.AccountNameTooLong);
    }

    [Fact]
    public void Account_Patch_WithEmptyBank_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);

        // Act
        var result = account.Patch("Patched Account", "", null, updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountErrors.AccountBankRequired);
    }

    [Fact]
    public void Account_Patch_WithTooLongBank_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);
        var newBank = AccountFixture.GenerateLongAccountBank();

        // Act
        var result = account.Patch("Patched Account", newBank, null, updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountErrors.AccountBankTooLong);
    }

    [Fact]
    public void Account_Patch_WithNewInitialBalance_ShouldUpdateInitialBalanceAndAccountBalance()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id, initialBalance: 100m);
        var updatedAt = FixtureBase.GetTestDate(1);
        const decimal newInitialBalance = 500m;

        // Act
        var result = account.Patch(account.Name, account.Bank, newInitialBalance, updatedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        account.InitialBalance.ShouldBe(newInitialBalance);
        account.Balance.ShouldBe(newInitialBalance);
        account.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void Account_Patch_WithNewInitialBalance_ShouldRecalculateAllOperationBalances()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var createdAt = FixtureBase.GetTestDate();
        var account = AccountFixture.CreateValidAccount(user.Id, initialBalance: 100m, createdAt: createdAt);

        // Add operations: +50, -30, +20 => Balance should be 100 + 50 - 30 + 20 = 140
        account.AddOperation("Operation 1", 50m, createdAt.AddMinutes(1), createdAt.AddMinutes(1));
        account.AddOperation("Operation 2", -30m, createdAt.AddMinutes(2), createdAt.AddMinutes(2));
        account.AddOperation("Operation 3", 20m, createdAt.AddMinutes(3), createdAt.AddMinutes(3));

        account.Balance.ShouldBe(140m);

        var updatedAt = FixtureBase.GetTestDate(1);
        const decimal newInitialBalance = 200m;

        // Act - Change initial balance from 100 to 200
        var result = account.Patch(account.Name, account.Bank, newInitialBalance, updatedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        account.InitialBalance.ShouldBe(200m);
        // New balance should be 200 + 50 - 30 + 20 = 240
        account.Balance.ShouldBe(240m);

        // Verify all operations have recalculated balances
        var operations = account.Operations.OrderBy(o => o.CreatedAt).ToList();

        // Operation 1: +50, previous = 200, next = 250
        operations[0].PreviousBalance.ShouldBe(200m);
        operations[0].NextBalance.ShouldBe(250m);

        // Operation 2: -30, previous = 250, next = 220
        operations[1].PreviousBalance.ShouldBe(250m);
        operations[1].NextBalance.ShouldBe(220m);

        // Operation 3: +20, previous = 220, next = 240
        operations[2].PreviousBalance.ShouldBe(220m);
        operations[2].NextBalance.ShouldBe(240m);
    }

    [Fact]
    public void Account_Patch_WithSameInitialBalance_ShouldNotRecalculateOperations()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var createdAt = FixtureBase.GetTestDate();
        var account = AccountFixture.CreateValidAccount(user.Id, initialBalance: 100m, createdAt: createdAt);

        account.AddOperation("Operation 1", 50m, createdAt.AddMinutes(1), createdAt.AddMinutes(1));

        var operation = account.Operations[0];
        var originalUpdatedAt = operation.UpdatedAt;

        var updatedAt = FixtureBase.GetTestDate(1);

        // Act - Patch with same initial balance
        var result = account.Patch("New Name", "New Bank", 100m, updatedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        account.Name.ShouldBe("New Name");
        account.Bank.ShouldBe("New Bank");
        account.InitialBalance.ShouldBe(100m);
        account.Balance.ShouldBe(150m);

        // Operation should not have been updated (same UpdatedAt)
        account.Operations[0].UpdatedAt.ShouldBe(originalUpdatedAt);
    }

    [Fact]
    public void Account_Patch_WithNullInitialBalance_ShouldNotUpdateInitialBalance()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id, initialBalance: 100m);
        var updatedAt = FixtureBase.GetTestDate(1);

        // Act
        var result = account.Patch("New Name", "New Bank", null, updatedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        account.Name.ShouldBe("New Name");
        account.Bank.ShouldBe("New Bank");
        account.InitialBalance.ShouldBe(100m);
        account.Balance.ShouldBe(100m);
    }

    [Fact]
    public void Account_Patch_WithNegativeInitialBalance_ShouldUpdateCorrectly()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var createdAt = FixtureBase.GetTestDate();
        var account = AccountFixture.CreateValidAccount(user.Id, initialBalance: 100m, createdAt: createdAt);

        account.AddOperation("Operation 1", 50m, createdAt.AddMinutes(1), createdAt.AddMinutes(1));

        var updatedAt = FixtureBase.GetTestDate(1);
        const decimal newInitialBalance = -100m;

        // Act
        var result = account.Patch(account.Name, account.Bank, newInitialBalance, updatedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        account.InitialBalance.ShouldBe(-100m);
        // Balance should be -100 + 50 = -50
        account.Balance.ShouldBe(-50m);

        var operation = account.Operations[0];
        operation.PreviousBalance.ShouldBe(-100m);
        operation.NextBalance.ShouldBe(-50m);
    }

    [Fact]
    public void Account_Patch_WithZeroInitialBalance_ShouldUpdateCorrectly()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var createdAt = FixtureBase.GetTestDate();
        var account = AccountFixture.CreateValidAccount(user.Id, initialBalance: 500m, createdAt: createdAt);

        account.AddOperation("Operation 1", 100m, createdAt.AddMinutes(1), createdAt.AddMinutes(1));

        var updatedAt = FixtureBase.GetTestDate(1);
        const decimal newInitialBalance = 0m;

        // Act
        var result = account.Patch(account.Name, account.Bank, newInitialBalance, updatedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        account.InitialBalance.ShouldBe(0m);
        // Balance should be 0 + 100 = 100
        account.Balance.ShouldBe(100m);

        var operation = account.Operations[0];
        operation.PreviousBalance.ShouldBe(0m);
        operation.NextBalance.ShouldBe(100m);
    }
}
