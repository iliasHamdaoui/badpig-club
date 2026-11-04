using BadPig.Shared.Core;

namespace BadPig.Usage.Features.ListUsageStatistics;

/// <summary>Search (page), sort (views, seconds, average, path; default views, most viewed first) and pagination.</summary>
/// <param name="Days">Period counted, ending today: 7, 30 (default), 90 or 365 days.</param>
public sealed record ListUsageStatisticsRequest : ListQuery
{
    public int? Days { get; init; }
}
