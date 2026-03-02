using Application.Interfaces;

namespace Application.Models;

public abstract record PaginatedQuery<TResponse>(
    int PageNumber = PaginationConstants.DefaultPageNumber,
    int PageSize = PaginationConstants.DefaultPageSize
) : IQuery<PaginatedList<TResponse>>;
