using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Queries.GetAccounts;

public sealed class GetAccountsHandler
    : IRequestHandler<GetAccountsQuery, List<GetAccountsResponse>>
{
    private readonly IAppDbContext _dbContext;

    public GetAccountsHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GetAccountsResponse>> Handle(
        GetAccountsQuery request,
        CancellationToken cancellationToken
    ) =>
        await _dbContext
            .Accounts.Select(account => new GetAccountsResponse(
                account.Id,
                account.Name,
                account.Balance,
                account.CreatedAt,
                account.UpdatedAt
            ))
            .ToListAsync(cancellationToken);
}
