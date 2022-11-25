using MediatR;

namespace Application.Features.Operations.UpdateOperation;

public sealed record UpdateOperationRequest : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string? Description { get; init; }
    public Guid[]? TagIds { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime? ExecutionDate { get; init; }
}