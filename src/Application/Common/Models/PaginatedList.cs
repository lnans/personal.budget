using Microsoft.EntityFrameworkCore;

namespace Application.Common.Models;

public sealed class InfiniteDataList<TItem>
{
    public List<TItem> Items { get; set; } = new();
    public int? NextCursor { get; set; }
}

public static class InfiniteDataList
{
    public static async Task<InfiniteDataList<TItem>> ToInfiniteDataListAsync<TItem>(this IQueryable<TItem> source, int cursor, int pageSize,
        CancellationToken ct = default)
    {
        var totalSize = await source.CountAsync(ct);
        var nextCursor = cursor + pageSize < totalSize ? cursor + pageSize : (int?)null;

        var items = await source.Skip(cursor).Take(pageSize).ToListAsync(ct);

        return new InfiniteDataList<TItem>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }
}