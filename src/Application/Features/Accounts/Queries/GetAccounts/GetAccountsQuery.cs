using MediatR;

namespace Application.Features.Accounts.Queries.GetAccounts;

public sealed class GetAccountsQuery : IRequest<List<GetAccountsResponse>> { }
