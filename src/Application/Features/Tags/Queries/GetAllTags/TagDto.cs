namespace Application.Features.Tags.Queries.GetAllTags;

public record TagDto
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Color { get; init; }
}