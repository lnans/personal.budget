using Domain.AccountOperations;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.AccountOperations;

public class AccountOperationUpdateDateTests
{
    [Fact]
    public void AccountOperation_UpdateDate_WithValidDate_ShouldUpdateOperationDate()
    {
        var createdAt = FixtureBase.GetTestDate();
        var operation = AccountOperationFixture.CreateValidAccountOperation(
            operationDate: createdAt,
            createdAt: createdAt
        );

        var newOperationDate = createdAt.AddDays(-1);
        var updatedAt = FixtureBase.GetTestDate(1);

        var result = operation.UpdateDate(newOperationDate, updatedAt);

        FixtureBase.AssertSuccess(result);
        operation.OperationDate.ShouldBe(newOperationDate);
        operation.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void AccountOperation_UpdateDate_WithFutureDate_ShouldReturnError()
    {
        var createdAt = FixtureBase.GetTestDate();
        var operation = AccountOperationFixture.CreateValidAccountOperation(
            operationDate: createdAt,
            createdAt: createdAt
        );

        var updatedAt = FixtureBase.GetTestDate(1);
        var futureOperationDate = updatedAt.AddDays(1);

        var result = operation.UpdateDate(futureOperationDate, updatedAt);

        FixtureBase.AssertError(result, AccountOperationErrors.AccountOperationDateInFuture);
        operation.OperationDate.ShouldBe(createdAt);
    }

    [Fact]
    public void AccountOperation_UpdateDate_WithDateEqualToUpdatedAt_ShouldSucceed()
    {
        var createdAt = FixtureBase.GetTestDate();
        var operation = AccountOperationFixture.CreateValidAccountOperation(
            operationDate: createdAt,
            createdAt: createdAt
        );

        var result = operation.UpdateDate(createdAt, createdAt);

        FixtureBase.AssertSuccess(result);
        operation.OperationDate.ShouldBe(createdAt);
    }

    [Fact]
    public void AccountOperation_UpdateDate_ShouldNotAffectAmountOrBalance()
    {
        var createdAt = FixtureBase.GetTestDate();
        var operation = AccountOperationFixture.CreateValidAccountOperation(
            operationDate: createdAt,
            createdAt: createdAt
        );

        var originalAmount = operation.Amount;
        var originalPreviousBalance = operation.PreviousBalance;
        var originalNextBalance = operation.NextBalance;
        var newOperationDate = createdAt.AddDays(-1);
        var updatedAt = FixtureBase.GetTestDate(1);

        var result = operation.UpdateDate(newOperationDate, updatedAt);

        FixtureBase.AssertSuccess(result);
        operation.Amount.ShouldBe(originalAmount);
        operation.PreviousBalance.ShouldBe(originalPreviousBalance);
        operation.NextBalance.ShouldBe(originalNextBalance);
    }
}
