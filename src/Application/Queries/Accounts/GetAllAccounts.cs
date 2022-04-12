using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Accounts;

public record GetAllAccountsRequest(string Name, bool Archived) : IRequest<IEnumerable<GetAllAccountsResponse>>;

public record GetAllAccountsResponse(string Id, string Name, string Icon, AccountType Type, decimal Balance, bool Archived, DateTime CreationDate);

public class GetAllAccounts : IRequestHandler<GetAllAccountsRequest, IEnumerable<GetAllAccountsResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetAllAccounts(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<IEnumerable<GetAllAccountsResponse>> Handle(GetAllAccountsRequest request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var accounts = _dbContext.Accounts.Where(a => a.OwnerId == userId && a.Archived == request.Archived);

        if (!string.IsNullOrWhiteSpace(request.Name))
            accounts = accounts.Where(a => a.Name.ToLower().Contains(request.Name.ToLower()));

        return await accounts
            .Select(a => new GetAllAccountsResponse(a.Id, a.Name, a.Icon, a.Type, a.Balance, a.Archived, a.CreationDate))
            .ToListAsync(cancellationToken);
    }
}