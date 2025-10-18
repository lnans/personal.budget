using Domain.AccountOperations;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.AccountOperations;

public class AccountOperationCreateTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateAccountOperation()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        const string operationDescription = "Test Operation";
        const decimal operationAmount = 100m;
        const decimal previousBalance = 50m;
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var result = AccountOperation.Create(
            accountId,
            operationDescription,
            operationAmount,
            previousBalance,
            createdAt
        );

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Description.ShouldBe(operationDescription);
        result.Value.Amount.ShouldBe(operationAmount);
        result.Value.PreviousBalance.ShouldBe(previousBalance);
        result.Value.NextBalance.ShouldBe(150m);
        result.Value.AccountId.ShouldBe(accountId);
        result.Value.CreatedAt.ShouldBe(createdAt);
        result.Value.UpdatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldReturnError()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        const string operationDescription = "";
        const decimal operationAmount = 100m;
        const decimal previousBalance = 0m;
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var result = AccountOperation.Create(
            accountId,
            operationDescription,
            operationAmount,
            previousBalance,
            createdAt
        );

        // Assert
        FixtureBase.AssertError(result, AccountOperationErrors.AccountOperationDescriptionRequired);
    }

    [Fact]
    public void Create_WithWhitespaceDescription_ShouldReturnError()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        const string operationDescription = "   ";
        const decimal operationAmount = 100m;
        const decimal previousBalance = 0m;
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var result = AccountOperation.Create(
            accountId,
            operationDescription,
            operationAmount,
            previousBalance,
            createdAt
        );

        // Assert
        FixtureBase.AssertError(result, AccountOperationErrors.AccountOperationDescriptionRequired);
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldReturnError()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var operationDescription = AccountOperationFixture.GenerateLongOperationDescription();
        const decimal operationAmount = 100m;
        const decimal previousBalance = 0m;
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var result = AccountOperation.Create(
            accountId,
            operationDescription,
            operationAmount,
            previousBalance,
            createdAt
        );

        // Assert
        FixtureBase.AssertError(result, AccountOperationErrors.AccountOperationDescriptionTooLong);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldCreateOperationAndCalculateCorrectBalance()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        const string operationDescription = "Withdrawal";
        const decimal operationAmount = -50m;
        const decimal previousBalance = 100m;
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var result = AccountOperation.Create(
            accountId,
            operationDescription,
            operationAmount,
            previousBalance,
            createdAt
        );

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Amount.ShouldBe(operationAmount);
        result.Value.PreviousBalance.ShouldBe(previousBalance);
        result.Value.NextBalance.ShouldBe(50m);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldCreateOperationWithSameBalance()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        const string operationDescription = "Zero operation";
        const decimal operationAmount = 0m;
        const decimal previousBalance = 100m;
        var createdAt = FixtureBase.GetTestDate();

        // Act
        var result = AccountOperation.Create(
            accountId,
            operationDescription,
            operationAmount,
            previousBalance,
            createdAt
        );

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Amount.ShouldBe(0m);
        result.Value.PreviousBalance.ShouldBe(previousBalance);
        result.Value.NextBalance.ShouldBe(previousBalance);
    }
}
