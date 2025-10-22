using Domain.AccountOperations;
using Domain.Users;
using ErrorOr;

namespace Domain.Accounts;

public sealed class Account : Entity
{
    public Guid UserId { get; }
    public string Name { get; private set; }
    public AccountType Type { get; }
    public decimal Balance { get; private set; }

    public User User { get; } = null!;
    private readonly ICollection<AccountOperation> _operations = [];
    public IReadOnlyList<AccountOperation> Operations => _operations.ToList().AsReadOnly();

    private Account(Guid userId, string name, AccountType type, decimal balance, DateTimeOffset createdAt)
        : base(createdAt)
    {
        UserId = userId;
        Name = name;
        Type = type;
        Balance = balance;
    }

    public static ErrorOr<Account> Create(
        Guid userId,
        string name,
        AccountType type,
        decimal balance,
        DateTimeOffset createdAt
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return AccountErrors.AccountNameRequired;
        }

        if (name.Length > AccountConstants.MaxNameLength)
        {
            return AccountErrors.AccountNameTooLong;
        }

        return new Account(userId, name, type, balance, createdAt);
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
