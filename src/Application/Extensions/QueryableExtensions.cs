using System.Linq.Expressions;
using Application.Models.Pagination;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Extensions;

internal static class QueryableExtensions
{
    extension<TEntity>(IQueryable<TEntity> source)
        where TEntity : class
    {
        internal async Task<ErrorOr<TEntity>> FirstOrErrorAsync(
            Error error,
            CancellationToken cancellationToken = default
        )
        {
            var entity = await source.FirstOrDefaultAsync(cancellationToken);
            return entity is null ? error : entity;
        }

        internal async Task<ErrorOr<TEntity>> FirstOrErrorAsync(
            Expression<Func<TEntity, bool>> predicate,
            Error error,
            CancellationToken cancellationToken = default
        )
        {
            var entity = await source.FirstOrDefaultAsync(predicate, cancellationToken);
            return entity is null ? error : entity;
        }

        internal async Task<ErrorOr<PaginatedList<TResult>>> ToPaginatedListOrErrorAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            int? pageNumber = PaginationConstants.DefaultPageNumber,
            int? pageSize = PaginationConstants.DefaultPageSize,
            CancellationToken cancellationToken = default
        )
        {
            var actualPageNumber = pageNumber ?? PaginationConstants.DefaultPageNumber;
            var actualPageSize = pageSize ?? PaginationConstants.DefaultPageSize;

            if (actualPageSize <= 0)
            {
                return PaginationErrors.PageSizeInvalid;
            }

            if (actualPageSize > PaginationConstants.MaxPageSize)
            {
                return PaginationErrors.PageSizeTooLarge;
            }

            if (actualPageNumber <= 0)
            {
                return PaginationErrors.PageNumberInvalid;
            }

            var offset = (long)(actualPageNumber - 1) * actualPageSize;
            if (offset > int.MaxValue)
            {
                return PaginationErrors.PageNumberTooLarge;
            }

            var count = await source.CountAsync(cancellationToken);
            var items = await source
                .Skip((int)offset)
                .Take(actualPageSize)
                .Select(selector)
                .ToListAsync(cancellationToken);

            return new PaginatedList<TResult>(items, actualPageNumber, actualPageSize, count);
        }

        internal IQueryable<TEntity> IncludeIf<TProperty>(
            bool condition,
            Expression<Func<TEntity, TProperty>> navigationPropertyPath
        ) => condition ? source.Include(navigationPropertyPath) : source;
    }
}
