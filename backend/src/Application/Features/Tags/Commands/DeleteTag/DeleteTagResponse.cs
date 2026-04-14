namespace Application.Features.Tags.Commands.DeleteTag;

public sealed record DeleteTagResponse(
    Guid Id,
    string Name,
    string Color,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset DeletedAt
);
