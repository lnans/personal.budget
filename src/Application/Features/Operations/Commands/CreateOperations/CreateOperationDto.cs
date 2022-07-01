using Domain.Enums;

namespace Application.Features.Operations.Commands.CreateOperations;

public record CreateOperationDto
{
    public string Description { get; init; }
    public string OperationTagId { get; init; }
    public OperationType OperationType { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime? ExecutionDate { get; init; }
}