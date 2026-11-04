namespace BadPig.Usage.Features.ListUsageStatistics;

/// <summary>One page of the website and how it was used over the period.</summary>
/// <param name="Path">The page as a route template (for example <c>/announcements/:id</c>).</param>
/// <param name="Views">How many times the page was opened.</param>
/// <param name="TotalSeconds">Total time the page stayed visible, in seconds.</param>
/// <param name="AverageSeconds">Average time per opening, in seconds (0 when the page was never opened).</param>
public sealed record ListUsageStatisticsItem(string Path, int Views, long TotalSeconds, int AverageSeconds);
