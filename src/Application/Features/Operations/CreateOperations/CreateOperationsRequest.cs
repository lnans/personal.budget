using Domain.Enums;
using MediatR;

namespace Application.Features.Operations.CreateOperations;

public sealed record CreateOperationsRequest : IRequest<Unit>
{
    public Guid AccountId { get; set; }
    public CreateOperationData[] Operations { get; set; } = Array.Empty<CreateOperationData>();
}

public sealed record CreateOperationData
{
    public string? Description { get; init; }
    public Guid[]? TagIds { get; init; }
    public OperationType Type { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime? ExecutionDate { get; init; }
}