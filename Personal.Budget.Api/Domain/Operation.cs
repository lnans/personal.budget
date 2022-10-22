using Personal.Budget.Api.Domain.Enums;

namespace Personal.Budget.Api.Domain;

public sealed class Operation
{
    public Guid Id { get; set; }
    public Account Account { get; set; } = default!;
    public string Description { get; set; } = default!;
    public OperationTag Tag { get; set; } = default!;
    public decimal Amount { get; set; }
    public OperationType Type { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? ExecutionDate { get; set; }
    public Guid CreatedById { get; set; }

    public User Owner { get; set; } = default!;
}