using Domain.Accounts;
using Domain.Tags;
using ErrorOr;

namespace Domain.AccountOperations;

public sealed class AccountOperation : Entity
{
    public Guid AccountId { get; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public decimal PreviousBalance { get; private set; }
    public decimal NextBalance { get; private set; }
    public bool IsRecurring { get; private set; }
    public DateTimeOffset OperationDate { get; private set; }
    public Account Account { get; } = null!;
    private readonly ICollection<Tag> _tags = [];
    public IReadOnlyList<Tag> Tags => _tags.ToList().AsReadOnly();

    private AccountOperation(
        Guid accountId,
        string description,
        decimal amount,
        decimal previousBalance,
        bool isRecurring,
        DateTimeOffset operationDate,
        DateTimeOffset createdAt
    )
        : base(createdAt)
    {
        AccountId = accountId;
        Description = description;
        Amount = amount;
        PreviousBalance = previousBalance;
        NextBalance = previousBalance + amount;
        IsRecurring = isRecurring;
        OperationDate = operationDate;
    }

    internal static ErrorOr<AccountOperation> Create(
        Guid accountId,
        string description,
        decimal amount,
        decimal previousBalance,
        bool isRecurring,
        DateTimeOffset operationDate,
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

        if (operationDate > createdAt)
        {
            return AccountOperationErrors.AccountOperationDateInFuture;
        }

        return new AccountOperation(
            accountId,
            description,
            amount,
            previousBalance,
            isRecurring,
            operationDate,
            createdAt
        );
    }

    public ErrorOr<AccountOperation> Rename(string description, DateTimeOffset updatedAt)
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
        return this;
    }

    public ErrorOr<AccountOperation> UpdateDate(DateTimeOffset operationDate, DateTimeOffset updatedAt)
    {
        if (operationDate > updatedAt)
        {
            return AccountOperationErrors.AccountOperationDateInFuture;
        }

        OperationDate = operationDate;
        UpdatedAt = updatedAt;
        return this;
    }

    public void UpdateRecurring(bool isRecurring, DateTimeOffset updatedAt)
    {
        IsRecurring = isRecurring;
        UpdatedAt = updatedAt;
    }

    public ErrorOr<AccountOperation> UpdateTags(IEnumerable<Tag> tags, DateTimeOffset updatedAt)
    {
        _tags.Clear();
        foreach (var tag in tags)
        {
            _tags.Add(tag);
        }
        UpdatedAt = updatedAt;
        return this;
    }

    internal void UpdateAmount(decimal newAmount, DateTimeOffset updatedAt)
    {
        Amount = newAmount;
        NextBalance = PreviousBalance + newAmount;
        UpdatedAt = updatedAt;
    }

    internal void UpdateBalances(decimal newPreviousBalance, DateTimeOffset updatedAt)
    {
        PreviousBalance = newPreviousBalance;
        NextBalance = newPreviousBalance + Amount;
        UpdatedAt = updatedAt;
    }

    internal void Delete(DateTimeOffset deletedAt)
    {
        NextBalance = PreviousBalance; // Operation no longer contributes to balance
        DeletedAt = deletedAt;
        UpdatedAt = deletedAt;
    }
}
