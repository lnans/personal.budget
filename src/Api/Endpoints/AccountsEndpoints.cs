using Api.Configurations;
using Api.Extensions;
using Application.Features.Accounts.Commands.AddOperation;
using Application.Features.Accounts.Commands.CreateAccount;
using Application.Features.Accounts.Commands.DeleteAccount;
using Application.Features.Accounts.Commands.RenameAccount;
using Application.Features.Accounts.Commands.RenameAccountOperation;
using Application.Features.Accounts.Commands.UpdateOperationAmount;
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
            .MapPatch("{id:guid}", RenameAccount)
            .WithDescription("Update an account name")
            .WithSummary("Update account")
            .Produces<RenameAccountResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(RenameAccount))
            .WithTags(Tag);

        group
            .MapPost("{id:guid}/operations", AddOperation)
            .WithDescription("Add an operation to an account")
            .WithSummary("Add operation")
            .Produces<AddOperationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(AddOperation))
            .WithTags(Tag);

        group
            .MapPatch("{accountId:guid}/operations/{operationId:guid}", RenameAccountOperation)
            .WithDescription("Rename an account operation")
            .WithSummary("Rename operation")
            .Produces<RenameAccountOperationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(RenameAccountOperation))
            .WithTags(Tag);

        group
            .MapPut("{accountId:guid}/operations/{operationId:guid}/amount", UpdateOperationAmount)
            .WithDescription("Update an account operation amount")
            .WithSummary("Update operation amount")
            .Produces<UpdateOperationAmountResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(UpdateOperationAmount))
            .WithTags(Tag);

        group
            .MapDelete("{id:guid}", DeleteAccount)
            .WithDescription("Delete an account (soft delete)")
            .WithSummary("Delete account")
            .Produces<DeleteAccountResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(DeleteAccount))
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

    private static async Task<IResult> RenameAccount(
        HttpContext context,
        IMediator mediator,
        Guid id,
        [FromBody] RenameAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> AddOperation(
        HttpContext context,
        IMediator mediator,
        Guid id,
        [FromBody] AddOperationCommand command,
        CancellationToken cancellationToken
    )
    {
        command.AccountId = id;
        var result = await mediator.Send(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> RenameAccountOperation(
        HttpContext context,
        IMediator mediator,
        Guid accountId,
        Guid operationId,
        [FromBody] RenameAccountOperationCommand command,
        CancellationToken cancellationToken
    )
    {
        command.AccountId = accountId;
        command.OperationId = operationId;
        var result = await mediator.Send(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> UpdateOperationAmount(
        HttpContext context,
        IMediator mediator,
        Guid accountId,
        Guid operationId,
        [FromBody] UpdateOperationAmountCommand command,
        CancellationToken cancellationToken
    )
    {
        command.AccountId = accountId;
        command.OperationId = operationId;
        var result = await mediator.Send(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> DeleteAccount(
        HttpContext context,
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var command = new DeleteAccountCommand { Id = id };
        var result = await mediator.Send(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }
}
