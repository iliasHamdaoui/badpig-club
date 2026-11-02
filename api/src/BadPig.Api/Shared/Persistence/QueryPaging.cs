using BadPig.Shared.Core;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shared.Persistence;

public static class QueryPaging
{
    /// <summary>The requested page of an already sorted query (the count and the page are two queries).</summary>
    public static async Task<PagedResponse<T>> ToPageAsync<T>(this IQueryable<T> sorted, ListQuery query, CancellationToken ct)
    {
        var size = query.PageLength();
        var total = await sorted.CountAsync(ct);
        var items = await sorted.Skip((query.PageNumber() - 1) * size).Take(size).ToListAsync(ct);
        return new PagedResponse<T>(items, query.PageNumber(), size, total);
    }
}
