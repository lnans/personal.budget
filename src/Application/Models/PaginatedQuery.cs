using Application.Interfaces;

namespace Application.Models;

public abstract class PaginatedQuery<TResponse> : IQuery<PaginatedList<TResponse>>
{
    public int PageNumber { get; init; } = PaginationConstants.DefaultPageNumber;
    public int PageSize { get; init; } = PaginationConstants.DefaultPageSize;
}
