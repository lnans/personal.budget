using Api.Configurations;
using Api.Contracts.Tags;
using Api.Extensions;
using Application.Features.Tags.Commands.CreateTag;
using Application.Features.Tags.Commands.DeleteTag;
using Application.Features.Tags.Commands.UpdateTag;
using Application.Features.Tags.Queries.GetTags;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class TagsEndpoints : IEndpoints
{
    private const string Tag = "Tags";

    public void MapEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/tags").RequireAuthorization();

        group
            .MapGet("", GetTags)
            .WithDescription("Get all tags")
            .WithSummary("Get all tags")
            .Produces<List<GetTagsResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(GetTags))
            .WithTags(Tag);

        group
            .MapPost("", CreateTag)
            .WithDescription("Create a new tag")
            .WithSummary("Create tag")
            .Produces<CreateTagResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(CreateTag))
            .WithTags(Tag);

        group
            .MapPut("{id:guid}", UpdateTag)
            .WithDescription("Update a tag")
            .WithSummary("Update tag")
            .Produces<UpdateTagResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(UpdateTag))
            .WithTags(Tag);

        group
            .MapDelete("{id:guid}", DeleteTag)
            .WithDescription("Delete a tag (soft delete)")
            .WithSummary("Delete tag")
            .Produces<DeleteTagResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(DeleteTag))
            .WithTags(Tag);
    }

    private static async Task<IResult> GetTags(
        HttpContext context,
        IQueryHandler<GetTagsQuery, List<GetTagsResponse>> handler,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(new GetTagsQuery(), cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> CreateTag(
        HttpContext context,
        ICommandHandler<CreateTagCommand, CreateTagResponse> handler,
        [FromBody] CreateTagRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateTagCommand(request.Name, request.Color);
        var result = await handler.Handle(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> UpdateTag(
        HttpContext context,
        ICommandHandler<UpdateTagCommand, UpdateTagResponse> handler,
        Guid id,
        [FromBody] UpdateTagRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdateTagCommand(id, request.Name, request.Color);
        var result = await handler.Handle(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> DeleteTag(
        HttpContext context,
        ICommandHandler<DeleteTagCommand, DeleteTagResponse> handler,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var command = new DeleteTagCommand(id);
        var result = await handler.Handle(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }
}
