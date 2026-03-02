using Api.Configurations;
using Api.Extensions;
using Application.Features.AccountOperations.Queries.GetPaginatedAccountOperations;
using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class AccountOperationsEndpoints : IEndpoints
{
    private const string Tag = "Operations";

    public void MapEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/operations").RequireAuthorization();

        group
            .MapGet("", GetPaginatedAccountOperations)
            .WithDescription("Get paginated account operations")
            .WithSummary("Get paginated operations")
            .Produces<PaginatedList<GetPaginatedAccountOperationsResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName(nameof(GetPaginatedAccountOperations))
            .WithTags(Tag);
    }

    private static async Task<IResult> GetPaginatedAccountOperations(
        HttpContext context,
        IQueryHandler<GetPaginatedAccountOperationsQuery, PaginatedList<GetPaginatedAccountOperationsResponse>> handler,
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

        var result = await handler.Handle(query, cancellationToken);
        return result.ToOkResultOrProblem(context);
    }
}
