using Domain.Enums;

namespace Application.Features.Transactions.Queries.GetPaginatedTransactions;

public record TransactionDto
{
    public string Id { get; init; }
    public string Description { get; init; }
    public string TagId { get; init; }
    public string TagName { get; init; }
    public string TagColor { get; init; }
    public TransactionType Type { get; init; }
    public string AccountId { get; init; }
    public string AccountName { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime? ExecutionDate { get; init; }
}