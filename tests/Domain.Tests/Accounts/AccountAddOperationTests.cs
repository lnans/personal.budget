using Domain.AccountOperations;

namespace Domain.Tests.Accounts;

public class AccountAddOperationTests : AccountTestsBase
{
    [Fact]
    public void AddOperation_WithValidParameters_ShouldAddOperationAndUpdateBalance()
    {
        // Arrange
        var account = CreateValidAccount();
        var updatedAt = GetTestDate(1);
        const decimal operationAmount = 10m;

        // Act
        var result = account.AddOperation(
            "Test Operation",
            operationAmount,
            updatedAt
        );

        // Assert
        AssertSuccess(result);
        account.Balance.ShouldBe(operationAmount);
        account.Operations.Count.ShouldBe(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddOperation_WithNullOrWhitespaceDescription_ShouldReturnError(
        string? description
    )
    {
        // Arrange
        var account = CreateValidAccount();
        var updatedAt = GetTestDate(1);

        // Act
        var result = account.AddOperation(description!, 10m, updatedAt);

        // Assert
        AssertError(
            result,
            AccountOperationErrors.AccountOperationDescriptionRequired
        );
        account.Balance.ShouldBe(0m);
        account.Operations.Count.ShouldBe(0);
    }

    [Fact]
    public void AddOperation_WithDescriptionTooLong_ShouldReturnError()
    {
        // Arrange
        var account = CreateValidAccount();
        var updatedAt = GetTestDate(1);
        var longDescription = GenerateLongString(
            AccountOperationConstants.MaxDescriptionLength + 1
        );

        // Act
        var result = account.AddOperation(longDescription, 10m, updatedAt);

        // Assert
        AssertError(
            result,
            AccountOperationErrors.AccountOperationDescriptionTooLong
        );
        account.Balance.ShouldBe(0m);
        account.Operations.Count.ShouldBe(0);
    }
}
