namespace Application.Features.OperationTags.Queries.GetAllOperationTags;

public record OperationTagDto
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Color { get; init; }
}