using System.Net;
using Api.Configurations;
using Application.Features.Accounts.ArchiveAccount;
using Application.Features.Accounts.CreateAccount;
using Application.Features.Accounts.DeleteAccount;
using Application.Features.Accounts.GetAccounts;
using Application.Features.Accounts.UpdateAccount;
using MediatR;

namespace Api.Endpoints;

internal class AccountsEndpoints : IEndPoints
{
    private const string Tag = "Accounts";

    public void Register(WebApplication app)
    {
        app.MapGet("accounts", GetAccounts)
            .RequireAuthorization()
            .Summary("Get accounts list", "Return all accounts")
            .ProduceResponse<IEnumerable<GetAccountsResponse>>(HttpStatusCode.OK, "Accounts list paginated")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .WithTags(Tag);

        app.MapPost("accounts", PostAccount)
            .RequireAuthorization()
            .Summary("Create an account", "Create a new account with an initial balance")
            .ProduceResponse(HttpStatusCode.NoContent, "Account created")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .ProduceError(HttpStatusCode.BadRequest, "Validation error occured")
            .ProduceError(HttpStatusCode.Conflict, "Account already exist")
            .WithTags(Tag);

        app.MapPatch("accounts/{id:guid}", PatchAccount)
            .RequireAuthorization()
            .Summary("Update an account", "Update name or bank of an account")
            .ProduceResponse(HttpStatusCode.NoContent, "Account updated")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .ProduceError(HttpStatusCode.BadRequest, "Validation error occured")
            .ProduceError(HttpStatusCode.NotFound, "Account not found")
            .WithTags(Tag);

        app.MapPatch("accounts/{id:guid}/archive", ArchiveAccount)
            .RequireAuthorization()
            .Summary("Archive an account", "Archive an account")
            .ProduceResponse(HttpStatusCode.NoContent, "Account archived")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .ProduceError(HttpStatusCode.NotFound, "Account not found")
            .WithTags(Tag);

        app.MapDelete("accounts/{id:guid}", DeleteAccount)
            .RequireAuthorization()
            .Summary("Delete an account", "Delete an account and all information associated")
            .ProduceResponse(HttpStatusCode.NoContent, "Account deleted")
            .ProduceError(HttpStatusCode.Unauthorized, "Authentication failed")
            .ProduceError(HttpStatusCode.NotFound, "Account not found")
            .WithTags(Tag);
    }

    private static async Task<IResult> GetAccounts(ISender mediator, [AsParameters] GetAccountsRequest request, CancellationToken ct) =>
        Results.Ok(await mediator.Send(request, ct));

    private static async Task<IResult> PostAccount(ISender mediator, CreateAccountRequest request, CancellationToken ct)
    {
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> PatchAccount(ISender mediator, Guid id, UpdateAccountRequest request, CancellationToken ct)
    {
        request.Id = id;
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ArchiveAccount(ISender mediator, Guid id, ArchiveAccountRequest request, CancellationToken ct)
    {
        request.Id = id;
        await mediator.Send(request, ct);
        return Results.NotFound();
    }

    private static async Task<IResult> DeleteAccount(ISender mediator, Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteAccountRequest { Id = id }, ct);
        return Results.NoContent();
    }
}