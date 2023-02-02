using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Accounts.GetAccounts;

internal sealed class GetAccountsQueryHandler : IRequestHandler<GetAccountsRequest, PaginatedList<GetAccountsResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthContext _authContext;

    public GetAccountsQueryHandler(IApplicationDbContext dbContext, IAuthContext authContext)
    {
        _authContext = authContext;
        _dbContext = dbContext;
    }

    public async Task<PaginatedList<GetAccountsResponse>> Handle(GetAccountsRequest request, CancellationToken cancellationToken)
    {
        var userId = _authContext.GetAuthenticatedUserId();
        var accounts = _dbContext.Accounts.Where(a => a.OwnerId == userId && a.Archived == request.Archived);

        if (!string.IsNullOrWhiteSpace(request.Search))
            accounts = accounts.Where(a => a.Name.ToLower().Contains(request.Search.ToLower()) || a.Bank.ToLower().Contains(request.Search.ToLower()));

        return await accounts
            .Select(a => new GetAccountsResponse
            {
                Id = a.Id,
                Archived = a.Archived,
                Balance = a.Balance,
                Bank = a.Bank,
                CreationDate = a.CreationDate,
                Name = a.Name,
                Type = a.Type
            })
            .OrderBy(a => a.Type)
            .ThenBy(a => a.Name.ToLower())
            .ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}