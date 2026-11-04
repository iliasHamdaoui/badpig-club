namespace BadPig.Usage.Features.GetUsageTrend;

/// <summary>The use of the website day by day, to draw a chart.</summary>
/// <param name="Days">One entry per day of the period, oldest first, today last; a day without any visit is there with zeros.</param>
public sealed record GetUsageTrendResponse(IReadOnlyList<GetUsageTrendDay> Days);

/// <summary>One day of the chart.</summary>
/// <param name="Date">The day, as yyyy-MM-dd (Paris).</param>
/// <param name="Views">Pages opened that day, all pages together.</param>
/// <param name="Seconds">Time the pages stayed visible that day, in seconds, all pages together.</param>
public sealed record GetUsageTrendDay(DateOnly Date, int Views, long Seconds);
