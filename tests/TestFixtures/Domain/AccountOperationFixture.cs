using Domain.AccountOperations;

namespace TestFixtures.Domain;

public static class AccountOperationFixture
{
    public static AccountOperation CreateValidAccountOperation(
        string description = "Test Operation",
        decimal amount = 100m,
        decimal previousBalance = 0m,
        DateTimeOffset? operationDate = null,
        DateTimeOffset? createdAt = null
    ) =>
        AccountOperation
            .Create(
                Guid.NewGuid(),
                description,
                amount,
                previousBalance,
                operationDate ?? FixtureBase.GetTestDate(),
                createdAt ?? FixtureBase.GetTestDate()
            )
            .Value;

    public static string GenerateLongOperationDescription() =>
        FixtureBase.GenerateLongString(AccountOperationConstants.MaxDescriptionLength + 1);
}
