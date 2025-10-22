using Domain.Accounts;

namespace TestFixtures.Domain;

public static class AccountFixture
{
    public static Account CreateValidAccount(
        Guid userId,
        string name = "Test Account",
        AccountType accountType = AccountType.Checking,
        decimal initialBalance = 0m,
        DateTimeOffset? createdAt = null
    ) => Account.Create(userId, name, accountType, initialBalance, createdAt ?? DateTimeOffset.UtcNow).Value;

    public static string GenerateLongAccountName() =>
        FixtureBase.GenerateLongString(AccountConstants.MaxNameLength + 1);
}
