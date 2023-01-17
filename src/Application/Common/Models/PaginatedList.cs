using Microsoft.EntityFrameworkCore;

namespace Application.Common.Models;

public sealed class PaginatedList<TItem>
{
    public int Page { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
    public List<TItem> Items { get; set; } = new();
}

public static class PaginatedList
{
    public static async Task<PaginatedList<TItem>> ToPaginatedListAsync<TItem>(this IQueryable<TItem> source, int page, int pageSize,
        CancellationToken ct = default)
    {
        var totalElements = await source.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalElements / (decimal)pageSize);

        var items = await source.Skip(page * pageSize).Take(pageSize).ToListAsync(ct);

        return new PaginatedList<TItem>
        {
            Page = page,
            TotalElements = totalElements,
            TotalPages = totalPages,
            Items = items
        };
    }
}