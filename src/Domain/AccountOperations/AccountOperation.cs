using ErrorOr;

namespace Domain.AccountOperations;

public sealed class AccountOperation : Entity
{
    public Guid AccountId { get; }
    public string Description { get; }
    public decimal Amount { get; }
    public decimal PreviousBalance { get; }
    public decimal NextBalance { get; }

    private AccountOperation(Guid accountId, string description, decimal amount, decimal previousBalance)
    {
        AccountId = accountId;
        Description = description;
        Amount = amount;
        PreviousBalance = previousBalance;
        NextBalance = previousBalance + amount;
    }

    internal static ErrorOr<AccountOperation> Create(Guid accountId, string description, decimal amount, decimal previousBalance)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return AccountOperationErrors.AccountOperationDescriptionRequired;
        }

        if (description.Length > AccountOperationConstants.MaxDescriptionLength)
        {
            return AccountOperationErrors.AccountOperationDescriptionTooLong;
        }

        return new AccountOperation(accountId, description, amount, previousBalance);
    }
}
