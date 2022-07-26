using Application.Features.Tags.Commands.CreateTag;
using Application.Features.Tags.Commands.DeleteTag;
using Application.Features.Tags.Commands.PatchTag;
using Application.Features.Tags.Queries.GetAllTags;

namespace Api.EndPoints;

public class TagsEndPoints : IEndPoints
{
    private const string Tag = "Tags";

    public void Register(WebApplication app)
    {
        app.MapGet("tags", GetAll)
            .Summary("Get tags", "Get tags list based on query search parameters")
            .Response<IEnumerable<TagDto>>(200, "Tag list")
            .Response<ErrorResponse>(401, "User not logged")
            .WithTags(Tag);

        app.MapPost("tags", Create)
            .Summary("Create tag", "Create a new Tag, name must be unique, color must be valid hexadecimal code")
            .Response(204, "Tag created")
            .Response<ErrorResponse>(400, "Model validation error occurs")
            .Response<ErrorResponse>(401, "User not logged")
            .WithTags(Tag);

        app.MapPut("tags/{id}", Update)
            .Summary("Update tag", "Update an existing Tag, name must be unique, color must be valid hexadecimal code")
            .Response(204, "Tag updated")
            .Response<ErrorResponse>(400, "Model validation error occurs")
            .Response<ErrorResponse>(401, "User not logged")
            .WithTags(Tag);

        app.MapDelete("tags/{id}", Delete)
            .Summary("Delete tag", "Delete an existing tag")
            .Response(204, "Tag deleted")
            .Response<ErrorResponse>(401, "User not logged")
            .Response<ErrorResponse>(404, "Tag not found")
            .WithTags(Tag);
    }

    private static async Task<IResult> GetAll(IMediator mediator, [FromQuery] string name, CancellationToken ct) =>
        Results.Ok(await mediator.Send(new GetAllTagsRequest {Name = name}, ct));

    private static async Task<IResult> Create(IMediator mediator, CreateTagRequest request, CancellationToken ct)
    {
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Update(IMediator mediator, string id, PatchTagRequest request, CancellationToken ct)
    {
        request.Id = id;
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Delete(IMediator mediator, string id, CancellationToken ct)
    {
        await mediator.Send(new DeleteTagRequest {Id = id}, ct);
        return Results.NoContent();
    }
}