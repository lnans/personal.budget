using Domain.Accounts;
using ErrorOr;

namespace Domain.AccountOperations;

public sealed class AccountOperation : Entity
{
    public Guid AccountId { get; }
    public string Description { get; private set; }
    public decimal Amount { get; }
    public decimal PreviousBalance { get; }
    public decimal NextBalance { get; }
    public Account Account { get; } = null!;

    private AccountOperation(
        Guid accountId,
        string description,
        decimal amount,
        decimal previousBalance,
        DateTimeOffset createdAt
    )
        : base(createdAt)
    {
        AccountId = accountId;
        Description = description;
        Amount = amount;
        PreviousBalance = previousBalance;
        NextBalance = previousBalance + amount;
    }

    internal static ErrorOr<AccountOperation> Create(
        Guid accountId,
        string description,
        decimal amount,
        decimal previousBalance,
        DateTimeOffset createdAt
    )
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return AccountOperationErrors.AccountOperationDescriptionRequired;
        }

        if (description.Length > AccountOperationConstants.MaxDescriptionLength)
        {
            return AccountOperationErrors.AccountOperationDescriptionTooLong;
        }

        return new AccountOperation(accountId, description, amount, previousBalance, createdAt);
    }

    public ErrorOr<Success> Rename(string description, DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return AccountOperationErrors.AccountOperationDescriptionRequired;
        }

        if (description.Length > AccountOperationConstants.MaxDescriptionLength)
        {
            return AccountOperationErrors.AccountOperationDescriptionTooLong;
        }

        Description = description;
        UpdatedAt = updatedAt;
        return Result.Success;
    }

    internal void Delete(DateTimeOffset deletedAt)
    {
        DeletedAt = deletedAt;
        UpdatedAt = deletedAt;
    }
}
