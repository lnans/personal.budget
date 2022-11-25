using MediatR;

namespace Application.Features.Tags.CreateTag;

public sealed record CreateTagRequest : IRequest<Guid>
{
    public string? Name { get; init; }
    public string? Color { get; init; }
}