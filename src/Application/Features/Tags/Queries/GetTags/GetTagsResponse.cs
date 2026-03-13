namespace Application.Features.Tags.Queries.GetTags;

public sealed record GetTagsResponse(
    Guid Id,
    string Name,
    string Color,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
