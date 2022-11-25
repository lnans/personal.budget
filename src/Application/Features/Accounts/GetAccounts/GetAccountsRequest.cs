using MediatR;

namespace Application.Features.Accounts.GetAccounts;

public sealed record GetAccountsRequest : IRequest<IEnumerable<GetAccountsResponse>>
{
    public bool Archived { get; set; }
}