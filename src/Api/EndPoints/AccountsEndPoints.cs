using Application.Features.Accounts.Commands.ArchivedAccount;
using Application.Features.Accounts.Commands.CreateAccount;
using Application.Features.Accounts.Commands.DeleteAccount;
using Application.Features.Accounts.Commands.PutAccount;
using Application.Features.Accounts.Queries.GetAllAccounts;

namespace Api.EndPoints;

public class AccountsEndPoints : IEndPoints
{
    private const string Tag = "Accounts";

    public void Register(WebApplication app)
    {
        app.MapGet("accounts", GetAll)
            .Summary("Accounts list", "Return accounts list base on query parameters")
            .Response<IEnumerable<AccountDto>>(200, "Accounts list")
            .Response<ErrorResponse>(401, "User not logged")
            .WithTags(Tag);

        app.MapPost("accounts", Create)
            .Summary("Create account", "Create a new accounts")
            .Response(204, "Account created")
            .Response<ErrorResponse>(400, "Model validation error occurs")
            .Response<ErrorResponse>(401, "User not logged")
            .WithTags(Tag);

        app.MapPut("accounts/{id}", Update)
            .Summary("Update account", "Update name and bank of an account")
            .Response(204, "Account updated")
            .Response<ErrorResponse>(400, "Model validation error occurs")
            .Response<ErrorResponse>(401, "User not logged")
            .WithTags(Tag);

        app.MapPut("accounts/{id}/archived", Archived)
            .Summary("Archive account", "Switch archived state from an accounts")
            .Response(204, "Account updated")
            .Response<ErrorResponse>(400, "Model validation error occurs")
            .Response<ErrorResponse>(401, "User not logged")
            .WithTags(Tag);

        app.MapDelete("accounts/{id}", Delete)
            .Summary("Delete account", "Delete an existing account")
            .Response(204, "Account deleted")
            .Response<ErrorResponse>(401, "User not logged")
            .Response<ErrorResponse>(404, "Account not found")
            .WithTags(Tag);
    }

    private static async Task<IResult> GetAll(IMediator mediator, [FromQuery] string name, [FromQuery] bool archived, CancellationToken ct) =>
        Results.Ok(await mediator.Send(new GetAllAccountsRequest {Name = name, Archived = archived}, ct));

    private static async Task<IResult> Create(IMediator mediator, CreateAccountRequest request, CancellationToken ct)
    {
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Update(IMediator mediator, string id, [FromBody] PutAccountRequest request, CancellationToken ct)
    {
        request.Id = id;
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Archived(IMediator mediator, string id, [FromBody] ArchivedAccountRequest request, CancellationToken ct)
    {
        request.Id = id;
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Delete(IMediator mediator, string id, CancellationToken ct)
    {
        await mediator.Send(new DeleteAccountRequest {Id = id}, ct);
        return Results.NoContent();
    }
}