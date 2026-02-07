using Api.Configurations;
using Api.Extensions;
using Application.Features.Accounts.Commands.AddAccountOperation;
using Application.Features.Accounts.Commands.CreateAccount;
using Application.Features.Accounts.Commands.DeleteAccount;
using Application.Features.Accounts.Commands.DeleteAccountOperation;
using Application.Features.Accounts.Commands.PatchAccount;
using Application.Features.Accounts.Commands.RenameAccountOperation;
using Application.Features.Accounts.Commands.UpdateAccountOperationAmount;
using Application.Features.Accounts.Queries.GetAccounts;
using Application.Features.Accounts.Queries.GetPaginatedAccountOperations;
using Application.Models;
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
            .MapGet("operations", GetPaginatedAccountOperations)
            .WithDescription("Get paginated account operations")
            .WithSummary("Get paginated operations")
            .Produces<PaginatedList<GetPaginatedAccountOperationsResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(GetPaginatedAccountOperations))
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
            .MapPatch("{id:guid}", PatchAccount)
            .WithDescription("Update an account")
            .WithSummary("Update account")
            .Produces<PatchAccountResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(PatchAccount))
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
            .MapPatch("{accountId:guid}/operations/{operationId:guid}", RenameAccountOperation)
            .WithDescription("Rename an account operation")
            .WithSummary("Rename operation")
            .Produces<RenameAccountOperationResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(RenameAccountOperation))
            .WithTags(Tag);

        group
            .MapPut("{accountId:guid}/operations/{operationId:guid}/amount", UpdateAccountOperationAmount)
            .WithDescription("Update an account operation amount")
            .WithSummary("Update operation amount")
            .Produces<UpdateAccountOperationAmountResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(UpdateAccountOperationAmount))
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

    private static async Task<IResult> GetPaginatedAccountOperations(
        HttpContext context,
        IMediator mediator,
        [FromQuery] string? accountId,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken
    )
    {
        _ = Guid.TryParse(accountId, out var parsedAccountId);

        var query = new GetPaginatedAccountOperationsQuery
        {
            AccountId = accountId is not null ? parsedAccountId : null,
            PageNumber = pageNumber ?? PaginationConstants.DefaultPageNumber,
            PageSize = pageSize ?? PaginationConstants.DefaultPageSize,
        };

        var result = await mediator.Send(query, cancellationToken);

        return result.ToOkResultOrProblem(context);
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

    private static async Task<IResult> AddAccountOperation(
        HttpContext context,
        IMediator mediator,
        Guid id,
        [FromBody] AddAccountOperationCommand command,
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

    private static async Task<IResult> UpdateAccountOperationAmount(
        HttpContext context,
        IMediator mediator,
        Guid accountId,
        Guid operationId,
        [FromBody] UpdateAccountOperationAmountCommand command,
        CancellationToken cancellationToken
    )
    {
        command.AccountId = accountId;
        command.OperationId = operationId;
        var result = await mediator.Send(command, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }

    private static async Task<IResult> DeleteAccountOperation(
        HttpContext context,
        IMediator mediator,
        Guid accountId,
        Guid operationId,
        CancellationToken cancellationToken
    )
    {
        var command = new DeleteAccountOperationCommand { AccountId = accountId, OperationId = operationId };
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
