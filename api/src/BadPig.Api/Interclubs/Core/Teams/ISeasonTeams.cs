namespace BadPig.Interclubs.Core;

/// <summary>
/// What the other modules may ask about the teams of a season: the Administration refuses to delete a season
/// that still has interclub teams, without reading the tables of this module.
/// </summary>
public interface ISeasonTeams
{
    /// <summary>True when at least one team plays in this season.</summary>
    Task<bool> ExistInAsync(Guid seasonId, CancellationToken ct);
}
