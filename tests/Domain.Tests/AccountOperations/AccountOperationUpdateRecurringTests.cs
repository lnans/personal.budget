using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.AccountOperations;

public class AccountOperationUpdateRecurringTests
{
    [Fact]
    public void AccountOperation_UpdateRecurring_WithTrue_ShouldSetIsRecurring()
    {
        var operation = AccountOperationFixture.CreateValidAccountOperation();
        var updatedAt = FixtureBase.GetTestDate(1);

        operation.UpdateRecurring(true, updatedAt);

        operation.IsRecurring.ShouldBeTrue();
        operation.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void AccountOperation_UpdateRecurring_WithFalse_ShouldUnsetIsRecurring()
    {
        var operation = AccountOperationFixture.CreateValidAccountOperation(isRecurring: true);
        var updatedAt = FixtureBase.GetTestDate(1);

        operation.UpdateRecurring(false, updatedAt);

        operation.IsRecurring.ShouldBeFalse();
        operation.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void AccountOperation_UpdateRecurring_ShouldNotChangeAmountOrBalance()
    {
        var operation = AccountOperationFixture.CreateValidAccountOperation();
        var originalAmount = operation.Amount;
        var originalPreviousBalance = operation.PreviousBalance;
        var originalNextBalance = operation.NextBalance;
        var updatedAt = FixtureBase.GetTestDate(1);

        operation.UpdateRecurring(true, updatedAt);

        operation.Amount.ShouldBe(originalAmount);
        operation.PreviousBalance.ShouldBe(originalPreviousBalance);
        operation.NextBalance.ShouldBe(originalNextBalance);
    }

    [Fact]
    public void AccountOperation_UpdateRecurring_ShouldNotChangeDescription()
    {
        var operation = AccountOperationFixture.CreateValidAccountOperation();
        var originalDescription = operation.Description;
        var updatedAt = FixtureBase.GetTestDate(1);

        operation.UpdateRecurring(true, updatedAt);

        operation.Description.ShouldBe(originalDescription);
    }
}
