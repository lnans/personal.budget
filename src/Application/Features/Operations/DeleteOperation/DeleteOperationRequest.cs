using MediatR;

namespace Application.Features.Operations.DeleteOperation;

public sealed record DeleteOperationRequest : IRequest<Unit>
{
    public Guid Id { get; set; }
}