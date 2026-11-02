namespace BadPig.Interclubs.Features.ListInterclubSeasons;

/// <summary>A season in which the club has teams.</summary>
/// <param name="Id">Identifier of the season.</param>
/// <param name="Name">Name of the season, e.g. "2026-2027".</param>
/// <param name="IsCurrent">True for the current season of the club (the one the site selects by default).</param>
/// <param name="TeamCount">Number of teams of the club in this season.</param>
public sealed record ListInterclubSeasonsItem(string Id, string Name, bool IsCurrent, int TeamCount);
