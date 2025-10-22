using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Queries.GetAccounts;

public sealed class GetAccountsHandler : IRequestHandler<GetAccountsQuery, List<GetAccountsResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuthContext _authContext;

    public GetAccountsHandler(IAppDbContext dbContext, IAuthContext authContext)
    {
        _dbContext = dbContext;
        _authContext = authContext;
    }

    public async Task<List<GetAccountsResponse>> Handle(
        GetAccountsQuery request,
        CancellationToken cancellationToken
    ) =>
        await _dbContext
            .Accounts.Where(account => account.UserId == _authContext.CurrentUserId)
            .Select(account => new GetAccountsResponse(
                account.Id,
                account.Name,
                account.Type,
                account.Balance,
                account.CreatedAt,
                account.UpdatedAt
            ))
            .ToListAsync(cancellationToken);
}
