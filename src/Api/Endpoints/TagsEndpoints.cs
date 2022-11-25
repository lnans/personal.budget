using System.Net;
using Api.Configurations;
using Application.Features.Tags.CreateTag;
using Application.Features.Tags.DeleteTag;
using Application.Features.Tags.GetTags;
using Application.Features.Tags.UpdateTag;
using MediatR;

namespace Api.Endpoints;

internal class TagsEndpoints : IEndPoints
{
    private const string Tag = "Tags";

    public void Register(WebApplication app)
    {
        app.MapGet("tags", GetTags)
            .RequireAuthorization()
            .Summary("Get all tags", "Return the list of user tags")
            .ProduceResponse<IEnumerable<GetTagsResponse>>(HttpStatusCode.OK, "Tags list")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .WithTags(Tag);

        app.MapPost("tags/{id:guid}", PostTag)
            .RequireAuthorization()
            .Summary("Create a tag", "Create a new tag for the current user")
            .ProduceResponse(HttpStatusCode.NoContent, "Tag created")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .ProduceError(HttpStatusCode.BadRequest, "Validation error occured")
            .ProduceError(HttpStatusCode.Conflict, "Tag already exist")
            .WithTags(Tag);

        app.MapPatch("tags/{id:guid}", PatchTag)
            .RequireAuthorization()
            .Summary("Update a tag", "Update name or color of a tag")
            .ProduceResponse(HttpStatusCode.NoContent, "Tag updated")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .ProduceError(HttpStatusCode.BadRequest, "Validation error occured")
            .ProduceError(HttpStatusCode.NotFound, "Tag not found")
            .WithTags(Tag);

        app.MapDelete("tags/{id:guid}", DeleteTag)
            .RequireAuthorization()
            .Summary("Delete a tag", "Delete an existing tag")
            .ProduceResponse(HttpStatusCode.NoContent, "Tag deleted")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .ProduceError(HttpStatusCode.NotFound, "Tag not found")
            .WithTags(Tag);
    }

    private static async Task<IResult> GetTags(ISender mediator, CancellationToken ct) =>
        Results.Ok(await mediator.Send(new GetTagsRequest(), ct));

    private static async Task<IResult> PostTag(ISender mediator, CreateTagRequest request, CancellationToken ct)
    {
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> PatchTag(ISender mediator, Guid id, UpdateTagRequest request, CancellationToken ct)
    {
        request.Id = id;
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteTag(ISender mediator, Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteTagRequest(), ct);
        return Results.NoContent();
    }
}