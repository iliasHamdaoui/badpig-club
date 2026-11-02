namespace BadPig.Administration.Core;

/// <summary>Checks that a season does not collide with the others (used when a season is created or changed).</summary>
public static class SeasonConflicts
{
    /// <summary>A message for the board when the name or the dates collide with another season; null when all is well.</summary>
    public static string? Find(IEnumerable<Season> others, string name, DateOnly startsOn, DateOnly endsOn)
    {
        foreach (var other in others)
        {
            if (string.Equals(other.Name, name, StringComparison.OrdinalIgnoreCase))
                return $"Une saison « {other.Name} » existe déjà.";

            if (SeasonRules.Overlaps(startsOn, endsOn, other))
                return $"Ces dates chevauchent la saison « {other.Name} ».";
        }

        return null;
    }
}
