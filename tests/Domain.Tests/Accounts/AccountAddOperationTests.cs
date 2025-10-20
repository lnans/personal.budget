using Domain.AccountOperations;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.Accounts;

public class AccountAddOperationTests
{
    [Fact]
    public void Account_AddOperation_WithValidParameters_ShouldAddOperationAndUpdateBalance()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);
        const decimal operationAmount = 10m;

        // Act
        var result = account.AddOperation("Test Operation", operationAmount, updatedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        account.Balance.ShouldBe(operationAmount);
        account.Operations.Count.ShouldBe(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Account_AddOperation_WithNullOrWhitespaceDescription_ShouldReturnError(string? description)
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);

        // Act
        var result = account.AddOperation(description!, 10m, updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountOperationErrors.AccountOperationDescriptionRequired);
        account.Balance.ShouldBe(0m);
        account.Operations.Count.ShouldBe(0);
    }

    [Fact]
    public void Account_AddOperation_WithDescriptionTooLong_ShouldReturnError()
    {
        // Arrange
        var user = UserFixture.CreateValidUser();
        var account = AccountFixture.CreateValidAccount(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);
        var longDescription = AccountOperationFixture.GenerateLongOperationDescription();

        // Act
        var result = account.AddOperation(longDescription, 10m, updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountOperationErrors.AccountOperationDescriptionTooLong);
        account.Balance.ShouldBe(0m);
        account.Operations.Count.ShouldBe(0);
    }
}
