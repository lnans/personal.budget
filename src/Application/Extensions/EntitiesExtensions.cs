using System.Linq.Expressions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Extensions;

internal static class EntitiesExtensions
{
    public static async Task<ErrorOr<TEntity>> FirstOrErrorAsync<TEntity>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, bool>> predicate,
        Error error,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        var entity = await source.FirstOrDefaultAsync(predicate, cancellationToken);

        if (entity is null)
        {
            return error;
        }

        return entity;
    }
}
