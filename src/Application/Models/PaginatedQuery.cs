using ErrorOr;
using MediatR;

namespace Application.Models;

public abstract class PaginatedQuery<TResponse> : IRequest<ErrorOr<PaginatedList<TResponse>>>
{
    public int PageNumber { get; init; } = PaginationConstants.DefaultPageNumber;
    public int PageSize { get; init; } = PaginationConstants.DefaultPageSize;
}
