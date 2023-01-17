using System.Net;
using Api.Configurations;
using Application.Common.Models;
using Application.Features.Operations.CreateOperations;
using Application.Features.Operations.DeleteOperation;
using Application.Features.Operations.GetOperations;
using Application.Features.Operations.UpdateOperation;
using Domain.Enums;
using MediatR;

namespace Api.Endpoints;

internal class OperationsEndpoints : IEndPoints
{
    private const string Tag = "Operations";

    public void Register(WebApplication app)
    {
        app.MapGet("operations", GetOperations)
            .RequireAuthorization()
            .Summary("Get operations", "Return a fragment of operations list with next iterator")
            .ProduceResponse<PaginatedList<GetOperationsResponse>>(HttpStatusCode.OK, "Operation list with iterator information")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .WithTags(Tag);

        app.MapPost("operations", PostOperations)
            .RequireAuthorization()
            .Summary("Create operations", "Create multiple operation for a specific account")
            .ProduceResponse(HttpStatusCode.NoContent, "Operations created")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .ProduceError(HttpStatusCode.BadRequest, "Validation error occured")
            .ProduceError(HttpStatusCode.NotFound, "Account not found")
            .WithTags(Tag);

        app.MapPatch("operations/{id:guid}", PatchOperation)
            .RequireAuthorization()
            .Summary("Update an operation", "Update operation information")
            .ProduceResponse(HttpStatusCode.NoContent, "Operations updated")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .ProduceError(HttpStatusCode.BadRequest, "Validation error occured")
            .ProduceError(HttpStatusCode.NotFound, "Operation not found")
            .WithTags(Tag);

        app.MapDelete("operations/{id:guid}", DeleteOperation)
            .RequireAuthorization()
            .Summary("Delete an operation", "Delete an existing operation")
            .ProduceResponse(HttpStatusCode.NoContent, "Operations deleted")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .ProduceError(HttpStatusCode.NotFound, "Operation not found")
            .WithTags(Tag);
    }

    private static async Task<IResult> GetOperations(ISender mediator,
        Guid? accountId,
        string? description,
        Guid[]? tagIds,
        OperationType? type,
        int? page,
        int? pageSize,
        CancellationToken ct) =>
        Results.Ok(await mediator.Send(new GetOperationsRequest
        {
            AccountId = accountId,
            Description = description,
            TagIds = tagIds,
            Type = type,
            Page = page ?? 0,
            PageSize = pageSize ?? 50
        }, ct));

    private static async Task<IResult> PostOperations(ISender mediator, CreateOperationsRequest request, CancellationToken ct)
    {
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> PatchOperation(ISender mediator, Guid id, UpdateOperationRequest request, CancellationToken ct)
    {
        request.Id = id;
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteOperation(ISender mediator, Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteOperationRequest { Id = id }, ct);
        return Results.NoContent();
    }
}