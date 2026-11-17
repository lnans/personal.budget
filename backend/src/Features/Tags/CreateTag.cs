using Microsoft.AspNetCore.Http.HttpResults;
using Personal.Budget.App.Common;
using Personal.Budget.App.Persistence;
using Personal.Budget.App.Persistence.Entities;

namespace Personal.Budget.App.Features.Tags;

public sealed class CreateTag
{
    public sealed record TagRequest(string? Name, string? Color);

    public sealed record TagResponse(
        Guid Id,
        string Name,
        string Color,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt
    );

    public void MapEndpoint(IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/tags", Create).WithName(nameof(CreateTag)).WithTags("Tags");

    private static async Task<Results<Created<TagResponse>, ValidationProblem>> Create(
        TagRequest req,
        AppDbContext db,
        TimeProvider time,
        CancellationToken ct
    )
    {
        var result = await Tag.Create(req.Name, req.Color, time.GetUtcNow())
            .ThenAsync<Tag>(async tag =>
            {
                db.Tags.Add(tag);
                await db.SaveChangesAsync(ct);
                return tag;
            });

        return result.Match<Results<Created<TagResponse>, ValidationProblem>>(
            tag => TypedResults.Created($"/tags/{tag.Id}", new TagResponse(tag.Id, tag.Name, tag.Color, tag.CreatedAt, tag.UpdatedAt)),
            errors => TypedResults.ValidationProblem(Error.ToProblemErrors(errors))
        );
    }
}
