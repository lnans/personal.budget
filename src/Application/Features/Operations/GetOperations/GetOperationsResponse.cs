using Domain.Enums;

namespace Application.Features.Operations.GetOperations;

public sealed class GetOperationsResponse
{
    public Guid Id { get; init; }
    public string Description { get; init; } = null!;
    public IEnumerable<OperationTagData> Tags { get; set; } = null!;
    public OperationType Type { get; init; }
    public OperationAccountData Account { get; set; } = null!;
    public decimal Amount { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime? ExecutionDate { get; init; }
}

public sealed class OperationTagData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Color { get; set; } = null!;
}

public sealed class OperationAccountData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}