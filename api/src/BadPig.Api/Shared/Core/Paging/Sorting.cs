namespace BadPig.Shared.Core;

public static class Sorting
{
    /// <summary>Sorts on a key in the requested direction; <see cref="ThenSort"/> adds a tie-breaker.</summary>
    public static IOrderedEnumerable<T> SortBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> key, bool descending, IComparer<TKey>? comparer = null) =>
        descending ? items.OrderByDescending(key, comparer) : items.OrderBy(key, comparer);

    public static IOrderedEnumerable<T> ThenSort<T, TKey>(this IEnumerable<T> items, Func<T, TKey> key, IComparer<TKey>? comparer = null) =>
        ((IOrderedEnumerable<T>)items).ThenBy(key, comparer);
}
