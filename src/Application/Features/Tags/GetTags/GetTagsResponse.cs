namespace Application.Features.Tags.GetTags;

public sealed record GetTagsResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Color { get; init; } = null!;
}