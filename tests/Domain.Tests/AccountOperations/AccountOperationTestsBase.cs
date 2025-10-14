using Domain.AccountOperations;

namespace Domain.Tests.AccountOperations;

public abstract class AccountOperationTestsBase : TestBase
{
    protected static AccountOperation CreateValidAccountOperation(
        string description = "Test Operation",
        decimal amount = 100m,
        decimal previousBalance = 0m,
        DateTimeOffset? createdAt = null
    ) =>
        AccountOperation
            .Create(
                Guid.NewGuid(),
                description,
                amount,
                previousBalance,
                createdAt ?? GetTestDate()
            )
            .Value;

    protected static string GenerateLongOperationDescription() =>
        GenerateLongString(AccountOperationConstants.MaxDescriptionLength + 1);
}
