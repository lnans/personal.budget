namespace Application.Features.Tags.Commands.CreateTag;

public sealed record CreateTagResponse(
    Guid Id,
    string Name,
    string Color,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
