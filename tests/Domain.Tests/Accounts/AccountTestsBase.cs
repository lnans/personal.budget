using Domain.Accounts;

namespace Domain.Tests.Accounts;

public abstract class AccountTestsBase : TestBase
{
    protected static Account CreateValidAccount(
        string name = "Test Account",
        decimal initialBalance = 0m,
        DateTimeOffset? createdAt = null
    ) =>
        Account
            .Create(name, initialBalance, createdAt ?? DateTimeOffset.UtcNow)
            .Value;

    protected static string GenerateLongAccountName() =>
        GenerateLongString(AccountConstants.MaxNameLength + 1);
}
