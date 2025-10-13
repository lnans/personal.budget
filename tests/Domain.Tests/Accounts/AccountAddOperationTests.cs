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
}
