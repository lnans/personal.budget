using Domain.AccountOperations;
using Domain.Users;
using ErrorOr;

namespace Domain.Accounts;

public sealed class Account : Entity
{
    public Guid UserId { get; }
    public string Name { get; private set; }
    public string Bank { get; private set; }
    public AccountType Type { get; }
    public decimal InitialBalance { get; private set; }
    public decimal Balance { get; private set; }

    public User User { get; } = null!;
    private readonly ICollection<AccountOperation> _operations = [];
    public IReadOnlyList<AccountOperation> Operations => _operations.ToList().AsReadOnly();

    private Account(
        Guid userId,
        string name,
        string bank,
        AccountType type,
        decimal initialBalance,
        DateTimeOffset createdAt
    )
        : base(createdAt)
    {
        UserId = userId;
        Name = name;
        Bank = bank;
        Type = type;
        InitialBalance = initialBalance;
        Balance = initialBalance;
    }

    public static ErrorOr<Account> Create(
        Guid userId,
        string name,
        string bank,
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

        if (string.IsNullOrWhiteSpace(bank))
        {
            return AccountErrors.AccountBankRequired;
        }

        if (bank.Length > AccountConstants.MaxBankLength)
        {
            return AccountErrors.AccountBankTooLong;
        }

        return new Account(userId, name, bank, type, balance, createdAt);
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

    public ErrorOr<Success> Patch(string name, string bank, decimal? initialBalance, DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return AccountErrors.AccountNameRequired;
        }

        if (name.Length > AccountConstants.MaxNameLength)
        {
            return AccountErrors.AccountNameTooLong;
        }

        if (string.IsNullOrWhiteSpace(bank))
        {
            return AccountErrors.AccountBankRequired;
        }

        if (bank.Length > AccountConstants.MaxBankLength)
        {
            return AccountErrors.AccountBankTooLong;
        }

        Name = name;
        Bank = bank;

        if (initialBalance.HasValue && initialBalance.Value != InitialBalance)
        {
            UpdateInitialBalance(initialBalance.Value, updatedAt);
        }

        UpdatedAt = updatedAt;
        return Result.Success;
    }

    private void UpdateInitialBalance(decimal newInitialBalance, DateTimeOffset updatedAt)
    {
        InitialBalance = newInitialBalance;

        // Recalculate all operations' balances starting from the new initial balance
        var orderedOperations = _operations.Where(o => o.DeletedAt is null).OrderBy(o => o.CreatedAt).ToList();

        var currentBalance = newInitialBalance;
        foreach (var operation in orderedOperations)
        {
            operation.UpdateBalances(currentBalance, updatedAt);
            currentBalance = operation.NextBalance;
        }

        Balance = currentBalance;
    }

    public ErrorOr<Success> UpdateOperationAmount(Guid operationId, decimal newAmount, DateTimeOffset updatedAt)
    {
        var operation = _operations.FirstOrDefault(o => o.Id == operationId);
        if (operation is null)
        {
            return AccountOperationErrors.AccountOperationNotFound;
        }

        operation.UpdateAmount(newAmount, updatedAt);

        // Get all operations after this one
        var subsequentOperations = _operations
            .Where(o => o.CreatedAt > operation.CreatedAt)
            .OrderBy(o => o.CreatedAt)
            .ToList();

        // Cascade the balance update to all subsequent operations
        var currentBalance = operation.NextBalance;
        foreach (var subsequentOperation in subsequentOperations)
        {
            subsequentOperation.UpdateBalances(currentBalance, updatedAt);
            currentBalance = subsequentOperation.NextBalance;
        }

        Balance = subsequentOperations.Any() ? currentBalance : operation.NextBalance;

        return Result.Success;
    }

    public ErrorOr<Success> DeleteOperation(Guid operationId, DateTimeOffset deletedAt)
    {
        var operation = _operations.FirstOrDefault(o => o.Id == operationId);
        if (operation is null)
        {
            return AccountOperationErrors.AccountOperationNotFound;
        }

        // Mark the operation as deleted
        operation.Delete(deletedAt);

        // Get all operations after this one
        var subsequentOperations = _operations
            .Where(o => o.CreatedAt > operation.CreatedAt && o.DeletedAt is null)
            .OrderBy(o => o.CreatedAt)
            .ToList();

        // Recalculate balances for subsequent operations
        // The new previous balance for the first subsequent operation is the operation's previous balance
        var currentBalance = operation.PreviousBalance;
        foreach (var subsequentOperation in subsequentOperations)
        {
            subsequentOperation.UpdateBalances(currentBalance, deletedAt);
            currentBalance = subsequentOperation.NextBalance;
        }

        // Update account balance to the last operation's next balance, or the starting balance if no operations remain
        Balance = subsequentOperations.Any() ? currentBalance : operation.PreviousBalance;
        UpdatedAt = deletedAt;

        return Result.Success;
    }

    public ErrorOr<Success> Delete(DateTimeOffset deletedAt)
    {
        if (DeletedAt is not null)
        {
            return AccountErrors.AccountAlreadyDeleted;
        }

        DeletedAt = deletedAt;
        UpdatedAt = deletedAt;

        foreach (var operation in _operations)
        {
            operation.Delete(deletedAt);
        }

        return Result.Success;
    }
}
