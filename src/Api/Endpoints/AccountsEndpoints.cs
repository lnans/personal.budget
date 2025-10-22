using Api.Configurations;
using Api.Extensions;
using Application.Features.Accounts.Commands.CreateAccount;
using Application.Features.Accounts.Commands.PatchAccount;
using Application.Features.Accounts.Queries.GetAccounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class AccountsEndpoints : IEndPoints
{
    private const string Tag = "Accounts";

    public void MapEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/accounts").RequireAuthorization();

        group
            .MapGet("", GetAccounts)
            .WithDescription("Get all accounts")
            .WithSummary("Get all accounts")
            .Produces<List<GetAccountsResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(GetAccounts))
            .WithTags(Tag);

        group
            .MapPost("", CreateAccount)
            .WithDescription("Create a new account")
            .WithSummary("Create account")
            .Produces<CreateAccountResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(CreateAccount))
            .WithTags(Tag);

        group
            .MapPatch("{id:guid}", PatchAccount)
            .WithDescription("Update an account name")
            .WithSummary("Update account")
            .Produces<PatchAccountResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(PatchAccount))
            .WithTags(Tag);
    }

    private static async Task<IResult> GetAccounts(IMediator mediator, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetAccountsQuery(), cancellationToken);

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateAccount(
        HttpContext context,
        IMediator mediator,
        [FromBody] CreateAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> PatchAccount(
        HttpContext context,
        IMediator mediator,
        Guid id,
        [FromBody] PatchAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }
}
