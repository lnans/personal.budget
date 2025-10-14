using Domain.AccountOperations;

namespace Domain.Tests.AccountOperations;

public class AccountOperationRenameTests : AccountOperationTestsBase
{
    [Fact]
    public void Rename_WithValidParameters_ShouldRenameAccountOperation()
    {
        // Arrange
        var operation = CreateValidAccountOperation();
        var updatedAt = GetTestDate(1);
        const string newDescription = "Renamed Operation";

        // Act
        var result = operation.Rename(newDescription, updatedAt);

        // Assert
        AssertSuccess(result);
        operation.Description.ShouldBe(newDescription);
        operation.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void Rename_WithEmptyDescription_ShouldReturnError()
    {
        // Arrange
        var operation = CreateValidAccountOperation();
        var updatedAt = GetTestDate(1);
        var originalDescription = operation.Description;

        // Act
        var result = operation.Rename("", updatedAt);

        // Assert
        AssertError(
            result,
            AccountOperationErrors.AccountOperationDescriptionRequired
        );
        operation.Description.ShouldBe(originalDescription);
    }

    [Fact]
    public void Rename_WithWhitespaceDescription_ShouldReturnError()
    {
        // Arrange
        var operation = CreateValidAccountOperation();
        var updatedAt = GetTestDate(1);
        var originalDescription = operation.Description;

        // Act
        var result = operation.Rename("   ", updatedAt);

        // Assert
        AssertError(
            result,
            AccountOperationErrors.AccountOperationDescriptionRequired
        );
        operation.Description.ShouldBe(originalDescription);
    }

    [Fact]
    public void Rename_WithTooLongDescription_ShouldReturnError()
    {
        // Arrange
        var operation = CreateValidAccountOperation();
        var updatedAt = GetTestDate(1);
        var originalDescription = operation.Description;
        var newDescription = GenerateLongOperationDescription();

        // Act
        var result = operation.Rename(newDescription, updatedAt);

        // Assert
        AssertError(
            result,
            AccountOperationErrors.AccountOperationDescriptionTooLong
        );
        operation.Description.ShouldBe(originalDescription);
    }

    [Fact]
    public void Rename_ShouldNotChangeAmountOrBalance()
    {
        // Arrange
        var operation = CreateValidAccountOperation("Original");
        var originalAmount = operation.Amount;
        var originalPreviousBalance = operation.PreviousBalance;
        var originalNextBalance = operation.NextBalance;
        var updatedAt = GetTestDate(1);

        // Act
        var result = operation.Rename("New Description", updatedAt);

        // Assert
        AssertSuccess(result);
        operation.Amount.ShouldBe(originalAmount);
        operation.PreviousBalance.ShouldBe(originalPreviousBalance);
        operation.NextBalance.ShouldBe(originalNextBalance);
    }
}
