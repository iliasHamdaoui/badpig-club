namespace BadPig.Administration.Features.UpdateSeason;

/// <param name="Id">Season identifier (from the route).</param>
/// <param name="Body">New values of the season.</param>
public sealed record UpdateSeasonRequest(string Id, UpdateSeasonBody Body);
