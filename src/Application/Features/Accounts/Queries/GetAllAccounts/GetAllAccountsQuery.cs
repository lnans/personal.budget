using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Queries.GetAllAccounts;

public class GetAllAccountsRequest : IRequest<IEnumerable<AccountDto>>
{
    public string Name { get; init; }
    public bool Archived { get; init; }
}

public class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsRequest, IEnumerable<AccountDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpUserContext _httpUserContext;

    public GetAllAccountsQueryHandler(IApplicationDbContext dbContext, IHttpUserContext httpUserContext)
    {
        _dbContext = dbContext;
        _httpUserContext = httpUserContext;
    }

    public async Task<IEnumerable<AccountDto>> Handle(GetAllAccountsRequest request, CancellationToken cancellationToken)
    {
        var userId = _httpUserContext.GetUserId();
        var accounts = _dbContext.Accounts.Where(a => a.OwnerId == userId && a.Archived == request.Archived);

        if (!string.IsNullOrWhiteSpace(request.Name))
            accounts = accounts.Where(a => a.Name.ToLower().Contains(request.Name.ToLower()));

        return await accounts
            .Select(a => new AccountDto
            {
                Id = a.Id,
                Archived = a.Archived,
                Balance = a.Balance,
                Bank = a.Bank,
                CreationDate = a.CreationDate,
                Icon = a.Icon,
                Name = a.Name,
                Type = a.Type
            })
            .OrderBy(a => a.Type)
            .ThenBy(a => a.Name.ToLower())
            .ToListAsync(cancellationToken);
    }
}