using Domain.AccountOperations;
using ErrorOr;

namespace Domain.Accounts;

public sealed class Account : Entity
{
    private Account(string name, decimal balance, DateTimeOffset createdAt)
        : base(createdAt)
    {
        Name = name;
        Balance = balance;
    }

    public string Name { get; private set; }
    public decimal Balance { get; private set; }
    private readonly ICollection<AccountOperation> _operations = [];
    public IReadOnlyList<AccountOperation> Operations => _operations.ToList().AsReadOnly();

    public static ErrorOr<Account> Create(string name, decimal balance, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return AccountErrors.AccountNameRequired;
        }

        if (name.Length > AccountConstants.MaxNameLength)
        {
            return AccountErrors.AccountNameTooLong;
        }

        return new Account(name, balance, createdAt);
    }

    public ErrorOr<Success> AddOperation(string description, decimal amount, DateTimeOffset createdAt) =>
        AccountOperation
            .Create(Id, description, amount, Balance, createdAt)
            .MatchFirst<ErrorOr<Success>>(
                operation =>
                {
                    _operations.Add(operation);
                    Balance = operation.NextBalance;
                    return Result.Success;
                },
                error => error
            );

    public ErrorOr<Success> Rename(string name, DateTimeOffset updatedAt)
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
        UpdatedAt = updatedAt;
        return Result.Success;
    }
}
