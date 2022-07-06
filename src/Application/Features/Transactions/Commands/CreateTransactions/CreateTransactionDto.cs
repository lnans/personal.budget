using Domain.Enums;

namespace Application.Features.Transactions.Commands.CreateTransactions;

public record CreateTransactionDto
{
    public string Description { get; init; }
    public string TagId { get; init; }
    public TransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime? ExecutionDate { get; init; }
}