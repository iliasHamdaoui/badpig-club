using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListSeasonStatistics;

/// <summary>Search (season name), sort (startsOn, name, totalCount, totalRevenue; default startsOn desc) and pagination.</summary>
public sealed record ListSeasonStatisticsRequest : ListQuery;
