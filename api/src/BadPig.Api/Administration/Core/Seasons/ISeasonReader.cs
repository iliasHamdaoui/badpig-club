namespace BadPig.Administration.Core;

/// <summary>A season as the other modules see it: what identifies it and what names it, never its fees.</summary>
/// <param name="Id">Identifier of the season.</param>
/// <param name="Name">Name of the season, e.g. "2026-2027".</param>
/// <param name="StartsOn">First day of the season.</param>
/// <param name="EndsOn">Last day of the season.</param>
public sealed record SeasonInfo(Guid Id, string Name, DateOnly StartsOn, DateOnly EndsOn);

/// <summary>
/// Read access to the seasons, owned by the Administration module: the modules that attach something to a season
/// (the interclub teams, for instance) use this instead of reading the table themselves.
/// </summary>
public interface ISeasonReader
{
    /// <summary>All the seasons, the latest started first.</summary>
    Task<IReadOnlyList<SeasonInfo>> ListAsync(CancellationToken ct);

    /// <summary>The season of today (see <see cref="SeasonRules.Current"/>); null when no season has started.</summary>
    Task<SeasonInfo?> CurrentAsync(CancellationToken ct);

    /// <summary>One season; null when it does not exist.</summary>
    Task<SeasonInfo?> FindAsync(Guid id, CancellationToken ct);
}
