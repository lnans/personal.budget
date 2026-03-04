namespace Application.Models;

public abstract record PaginatedQuery(
    int PageNumber = PaginationConstants.DefaultPageNumber,
    int PageSize = PaginationConstants.DefaultPageSize
);
