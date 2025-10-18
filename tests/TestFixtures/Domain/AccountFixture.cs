using Domain.Accounts;

namespace TestFixtures.Domain;

public static class AccountFixture
{
    public static Account CreateValidAccount(
        string name = "Test Account",
        decimal initialBalance = 0m,
        DateTimeOffset? createdAt = null
    ) =>
        Account
            .Create(name, initialBalance, createdAt ?? DateTimeOffset.UtcNow)
            .Value;

    public static string GenerateLongAccountName() =>
        FixtureBase.GenerateLongString(AccountConstants.MaxNameLength + 1);
}
