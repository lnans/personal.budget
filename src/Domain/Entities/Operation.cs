using Domain.Enums;

namespace Domain.Entities;

public sealed class Operation
{
    public Guid Id { get; set; }
    public Account Account { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Amount { get; set; }
    public OperationType Type { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? ExecutionDate { get; set; }
    public string OwnerId { get; set; } = null!;
    public ICollection<Tag>? Tags { get; set; }
}