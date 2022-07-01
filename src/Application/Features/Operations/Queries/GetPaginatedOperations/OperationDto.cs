using Domain.Enums;

namespace Application.Features.Operations.Queries.GetPaginatedOperations;

public record OperationDto
{
    public string Id { get; init; }
    public string Description { get; init; }
    public string TagId { get; init; }
    public string TagName { get; init; }
    public string TagColor { get; init; }
    public OperationType Type { get; init; }
    public string AccountId { get; init; }
    public string AccountName { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime? ExecutionDate { get; init; }
}