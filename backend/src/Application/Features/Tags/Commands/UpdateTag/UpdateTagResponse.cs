namespace Application.Features.Tags.Commands.UpdateTag;

public sealed record UpdateTagResponse(
    Guid Id,
    string Name,
    string Color,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
