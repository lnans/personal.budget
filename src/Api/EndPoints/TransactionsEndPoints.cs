using Application.Features.Transactions.Commands.CreateTransactions;
using Application.Features.Transactions.Commands.DeleteTransaction;
using Application.Features.Transactions.Commands.PutTransaction;
using Application.Features.Transactions.Queries.GetPaginatedTransactions;
using Domain.Enums;

namespace Api.EndPoints;

public class TransactionsEndPoints : IEndPoints
{
    private const string Tag = "Transactions";

    public void Register(WebApplication app)
    {
        app.MapGet("transactions", GetAll)
            .Summary("Get transactions list", "Get transactions list paginated and filtered")
            .Response<InfiniteData<TransactionDto>>(200, "Transaction list paginated")
            .Response<ErrorResponse>(401, "User not logged")
            .WithTags(Tag);

        app.MapPost("transactions", Create)
            .Summary("Create a transactions", "Create new transactions for an account")
            .Response(204, "Transactions created")
            .Response<ErrorResponse>(400, "Model validation error occurs")
            .Response<ErrorResponse>(401, "User not logged")
            .Response<ErrorResponse>(404, "Account not found")
            .WithTags(Tag);

        app.MapPut("transactions/{id}", Update)
            .Summary("Update a transaction", "Update transaction information")
            .Response(204, "Transaction updated")
            .Response<ErrorResponse>(401, "User not logged")
            .Response<ErrorResponse>(404, "Transaction not found")
            .WithTags(Tag);

        app.MapDelete("transactions/{id}", Delete)
            .Summary("Delete a transaction", "Delete an exising transaction")
            .Response(204, "Transaction deleted")
            .Response<ErrorResponse>(401, "User not logged")
            .Response<ErrorResponse>(404, "Transactions not found")
            .WithTags(Tag);
    }

    private static async Task<IResult> GetAll(
        IMediator mediator,
        [FromQuery] string accountId,
        [FromQuery] string description,
        [FromQuery] string tagId,
        [FromQuery] TransactionType? type,
        [FromQuery] int? cursor,
        CancellationToken ct) =>
        Results.Ok(await mediator.Send(new GetPaginatedTransactionsRequest
        {
            AccountId = accountId,
            Description = description,
            TagId = tagId,
            Type = type,
            Cursor = cursor.GetValueOrDefault()
        }, ct));

    private static async Task<IResult> Create(IMediator mediator, CreateTransactionsRequest request, CancellationToken ct)
    {
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Update(IMediator mediator, string id, PutTransactionRequest request, CancellationToken ct)
    {
        request.Id = id;
        await mediator.Send(request, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Delete(IMediator mediator, string id, CancellationToken ct)
    {
        await mediator.Send(new DeleteTransactionRequest {Id = id}, ct);
        return Results.NoContent();
    }
}