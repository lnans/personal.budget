using Application.Common.Models;
using MediatR;

namespace Application.Features.Accounts.GetAccounts;

public sealed record GetAccountsRequest : IRequest<PaginatedList<GetAccountsResponse>>
{
    public bool Archived { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}