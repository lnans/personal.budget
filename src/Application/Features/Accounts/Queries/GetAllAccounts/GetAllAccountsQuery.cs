using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Queries.GetAllAccounts;

public class GetAllAccountsRequest : IRequest<IEnumerable<AccountDto>>
{
    public string Name { get; init; }
    public bool Archived { get; init; }

    public static bool TryParse(string query, out GetAllAccountsRequest queryObj)
    {
        var obj = TypeDescriptor.GetConverter(typeof(GetAllAccountsRequest));
        var value = obj.ConvertFrom(null, CultureInfo.InvariantCulture, query);
        queryObj = value as GetAllAccountsRequest;
        return true;
    }

    public static ValueTask<GetAllAccountsRequest> BindAsync(HttpContext context, ParameterInfo parameterInfo)
    {
        string addressStr = context.Request.Query["address"];

        return new ValueTask<GetAllAccountsRequest>();
    }
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