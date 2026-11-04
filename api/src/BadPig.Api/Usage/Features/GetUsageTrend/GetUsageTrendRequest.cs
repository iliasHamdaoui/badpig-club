namespace BadPig.Usage.Features.GetUsageTrend;

/// <summary>The use of the website day by day.</summary>
/// <param name="Days">Period counted, ending today: 7, 30 (default), 90 or 365 days.</param>
public sealed record GetUsageTrendRequest(int? Days);
