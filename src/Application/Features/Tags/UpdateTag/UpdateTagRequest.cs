using MediatR;

namespace Application.Features.Tags.UpdateTag;

public sealed record UpdateTagRequest : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string? Name { get; init; }
    public string? Color { get; init; }
}