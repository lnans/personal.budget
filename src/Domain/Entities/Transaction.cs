using Domain.Enums;

namespace Domain.Entities;

public class Transaction
{
    public string Id { get; set; }
    public Account Account { get; set; }
    public string Description { get; set; }
    public Tag Tag { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? ExecutionDate { get; set; }
    public string CreatedById { get; set; }

    public User CreatedBy { get; set; }
}