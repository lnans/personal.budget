using Application.Interfaces;

namespace Application.Features.Accounts.Queries.GetAccounts;

public sealed record GetAccountsQuery() : IQuery<List<GetAccountsResponse>>;
