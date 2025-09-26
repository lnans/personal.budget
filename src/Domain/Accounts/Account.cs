using Domain.AccountOperations;
using ErrorOr;

namespace Domain.Accounts;

public sealed class Account : Entity
{
    private Account(string name, decimal balance)
    {
        Name = name;
        Balance = balance;
    }

    public string Name { get; private set; }
    public decimal Balance { get; private set; }

    private readonly ICollection<AccountOperation> _operations = [];

    public static ErrorOr<Account> Create(string name, decimal balance)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return AccountErrors.AccountNameRequired;
        }

        if (name.Length > AccountConstants.MaxNameLength)
        {
            return AccountErrors.AccountNameTooLong;
        }

        return new Account(name, balance);
    }

    public ErrorOr<Success> AddOperation(string description, decimal amount) =>
        AccountOperation
            .Create(Id, description, amount, Balance)
            .MatchFirst<ErrorOr<Success>>(
                operation =>
                {
                    _operations.Add(operation);
                    UpdatedAt = DateTimeOffset.UtcNow;
                    Balance = operation.NextBalance;
                    return Result.Success;
                },
                error => error
            );

    public ErrorOr<Success> Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return AccountErrors.AccountNameRequired;
        }

        if (name.Length > AccountConstants.MaxNameLength)
        {
            return AccountErrors.AccountNameTooLong;
        }

        Name = name;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success;
    }
}
