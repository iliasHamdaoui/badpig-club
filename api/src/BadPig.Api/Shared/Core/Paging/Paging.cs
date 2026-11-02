namespace BadPig.Shared.Core;

public static class Paging
{
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    public static int PageNumber(this ListQuery query) => Math.Max(1, query.Page ?? 1);

    public static int PageLength(this ListQuery query) => Math.Clamp(query.PageSize ?? DefaultPageSize, 1, MaxPageSize);

    /// <summary>The search text, lower-cased and trimmed; null when there is nothing to look for.</summary>
    public static string? SearchTerm(this ListQuery query) =>
        string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim().ToLowerInvariant();

    /// <summary>True when the text contains the search term (case-insensitive).</summary>
    public static bool HasText(this string? text, string term) => text?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false;

    /// <summary>True when the sort direction is descending, or when no direction is given and <paramref name="defaultDescending"/> is true.</summary>
    public static bool IsDescending(this ListQuery query, bool defaultDescending = false) =>
        query.Direction is null ? defaultDescending : query.Direction == SortDirection.Desc;

    /// <summary>The requested page of an already sorted list held in memory.</summary>
    public static PagedResponse<T> ToPage<T>(this IEnumerable<T> sorted, ListQuery query)
    {
        var all = sorted.ToList();
        var size = query.PageLength();
        return new PagedResponse<T>(all.Skip((query.PageNumber() - 1) * size).Take(size).ToList(), query.PageNumber(), size, all.Count);
    }
}
