namespace BadPig.Administration.Features.DeleteSeason;

/// <param name="Id">Season identifier (from the route).</param>
public sealed record DeleteSeasonRequest(string Id);
