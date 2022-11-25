using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.GetAccounts;

internal sealed class GetAccountsQueryHandler : IRequestHandler<GetAccountsRequest, IEnumerable<GetAccountsResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetAccountsQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _userContext = userContext;
        _dbContext = dbContext;
    }


    public async Task<IEnumerable<GetAccountsResponse>> Handle(GetAccountsRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetAuthenticatedUserId();
        var accounts = _dbContext.Accounts.Where(a => a.OwnerId == userId && a.Archived == request.Archived);

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
            .ToListAsync(cancellationToken);
    }
}