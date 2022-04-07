using Domain.Enums;

namespace Domain.Entities;

public class Operation
{
    public string Id { get; set; }
    public Account Account { get; set; }
    public string Description { get; set; }
    public OperationTag Tag { get; set; }
    public Account TransferAccount { get; set; }
    public decimal Amount { get; set; }
    public OperationType Type { get; set; }
    public DateTime OperationDate { get; set; }
    public User CreatedBy { get; set; }
}