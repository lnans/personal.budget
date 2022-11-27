using MediatR;

namespace Application.Features.Tags.DeleteTag;

public sealed record DeleteTagRequest : IRequest<Unit>
{
    public Guid Id { get; set; }
}