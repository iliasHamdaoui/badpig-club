namespace BadPig.Shared.Core;

/// <summary>One page of a list, with what is needed to display a pager.</summary>
/// <param name="Items">Items of the requested page.</param>
/// <param name="Page">Page number, starting at 1.</param>
/// <param name="PageSize">Number of items per page.</param>
/// <param name="TotalCount">Number of items over all the pages (after the search filter).</param>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PagedResponse<TOut> Map<TOut>(Func<T, TOut> map) => new(Items.Select(map).ToList(), Page, PageSize, TotalCount);
}
