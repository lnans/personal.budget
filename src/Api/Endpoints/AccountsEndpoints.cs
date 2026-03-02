using Api.Configurations;
using Api.Extensions;
using Application.Features.AccountOperations.Commands.AddAccountOperation;
using Application.Features.AccountOperations.Commands.DeleteAccountOperation;
using Application.Features.AccountOperations.Commands.UpdateAccountOperation;
using Application.Features.Accounts.Commands.CreateAccount;
using Application.Features.Accounts.Commands.DeleteAccount;
using Application.Features.Accounts.Commands.UpdateAccount;
using Application.Features.Accounts.Queries.GetAccounts;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class AccountsEndpoints : IEndpoints
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
            .Produces<CreateAccountResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(CreateAccount))
            .WithTags(Tag);

        group
            .MapPut("{id:guid}", UpdateAccount)
            .WithDescription("Update an account")
            .WithSummary("Update account")
            .Produces<UpdateAccountResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(UpdateAccount))
            .WithTags(Tag);

        group
            .MapPost("{id:guid}/operations", AddAccountOperation)
            .WithDescription("Add an operation to an account")
            .WithSummary("Add operation")
            .Produces<AddAccountOperationResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(AddAccountOperation))
            .WithTags(Tag);

        group
            .MapPut("{accountId:guid}/operations/{operationId:guid}", UpdateAccountOperation)
            .WithDescription("Update an account operation")
            .WithSummary("Update operation")
            .Produces<UpdateAccountOperationResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(UpdateAccountOperation))
            .WithTags(Tag);

        group
            .MapDelete("{accountId:guid}/operations/{operationId:guid}", DeleteAccountOperation)
            .WithDescription("Delete an account operation (soft delete)")
            .WithSummary("Delete operation")
            .Produces<DeleteAccountOperationResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(DeleteAccountOperation))
            .WithTags(Tag);

        group
            .MapDelete("{id:guid}", DeleteAccount)
            .WithDescription("Delete an account (soft delete)")
            .WithSummary("Delete account")
            .Produces<DeleteAccountResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(DeleteAccount))
            .WithTags(Tag);
    }

    private static async Task<IResult> GetAccounts(
        HttpContext context,
        IQueryHandler<GetAccountsQuery, List<GetAccountsResponse>> handler,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(new GetAccountsQuery(), cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> CreateAccount(
        HttpContext context,
        ICommandHandler<CreateAccountCommand, CreateAccountResponse> handler,
        [FromBody] CreateAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> UpdateAccount(
        HttpContext context,
        ICommandHandler<UpdateAccountCommand, UpdateAccountResponse> handler,
        Guid id,
        [FromBody] UpdateAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        command.Id = id;
        var result = await handler.Handle(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> AddAccountOperation(
        HttpContext context,
        ICommandHandler<AddAccountOperationCommand, AddAccountOperationResponse> handler,
        Guid id,
        [FromBody] AddAccountOperationCommand command,
        CancellationToken cancellationToken
    )
    {
        command.AccountId = id;
        var result = await handler.Handle(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> UpdateAccountOperation(
        HttpContext context,
        ICommandHandler<UpdateAccountOperationCommand, UpdateAccountOperationResponse> handler,
        Guid accountId,
        Guid operationId,
        [FromBody] UpdateAccountOperationCommand command,
        CancellationToken cancellationToken
    )
    {
        command.AccountId = accountId;
        command.OperationId = operationId;
        var result = await handler.Handle(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> DeleteAccountOperation(
        HttpContext context,
        ICommandHandler<DeleteAccountOperationCommand, DeleteAccountOperationResponse> handler,
        Guid accountId,
        Guid operationId,
        CancellationToken cancellationToken
    )
    {
        var command = new DeleteAccountOperationCommand { AccountId = accountId, OperationId = operationId };
        var result = await handler.Handle(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> DeleteAccount(
        HttpContext context,
        ICommandHandler<DeleteAccountCommand, DeleteAccountResponse> handler,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var command = new DeleteAccountCommand { Id = id };
        var result = await handler.Handle(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }
}
