using Domain.AccountOperations;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.AccountOperations;

public class AccountOperationRenameTests
{
    [Fact]
    public void Rename_WithValidParameters_ShouldRenameAccountOperation()
    {
        // Arrange
        var operation = AccountOperationFixture.CreateValidAccountOperation();
        var updatedAt = FixtureBase.GetTestDate(1);
        const string newDescription = "Renamed Operation";

        // Act
        var result = operation.Rename(newDescription, updatedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        operation.Description.ShouldBe(newDescription);
        operation.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void Rename_WithEmptyDescription_ShouldReturnError()
    {
        // Arrange
        var operation = AccountOperationFixture.CreateValidAccountOperation();
        var updatedAt = FixtureBase.GetTestDate(1);
        var originalDescription = operation.Description;

        // Act
        var result = operation.Rename("", updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountOperationErrors.AccountOperationDescriptionRequired);
        operation.Description.ShouldBe(originalDescription);
    }

    [Fact]
    public void Rename_WithWhitespaceDescription_ShouldReturnError()
    {
        // Arrange
        var operation = AccountOperationFixture.CreateValidAccountOperation();
        var updatedAt = FixtureBase.GetTestDate(1);
        var originalDescription = operation.Description;

        // Act
        var result = operation.Rename("   ", updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountOperationErrors.AccountOperationDescriptionRequired);
        operation.Description.ShouldBe(originalDescription);
    }

    [Fact]
    public void Rename_WithTooLongDescription_ShouldReturnError()
    {
        // Arrange
        var operation = AccountOperationFixture.CreateValidAccountOperation();
        var updatedAt = FixtureBase.GetTestDate(1);
        var originalDescription = operation.Description;
        var newDescription = AccountOperationFixture.GenerateLongOperationDescription();

        // Act
        var result = operation.Rename(newDescription, updatedAt);

        // Assert
        FixtureBase.AssertError(result, AccountOperationErrors.AccountOperationDescriptionTooLong);
        operation.Description.ShouldBe(originalDescription);
    }

    [Fact]
    public void Rename_ShouldNotChangeAmountOrBalance()
    {
        // Arrange
        var operation = AccountOperationFixture.CreateValidAccountOperation("Original");
        var originalAmount = operation.Amount;
        var originalPreviousBalance = operation.PreviousBalance;
        var originalNextBalance = operation.NextBalance;
        var updatedAt = FixtureBase.GetTestDate(1);

        // Act
        var result = operation.Rename("New Description", updatedAt);

        // Assert
        FixtureBase.AssertSuccess(result);
        operation.Amount.ShouldBe(originalAmount);
        operation.PreviousBalance.ShouldBe(originalPreviousBalance);
        operation.NextBalance.ShouldBe(originalNextBalance);
    }
}
