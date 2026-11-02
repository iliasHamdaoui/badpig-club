namespace BadPig.Interclubs.Core;

/// <summary>The score of a fixture: the matches won by each side. Known only once the seven matches have an outcome.</summary>
public readonly record struct MatchScore(int Club, int Opponent)
{
    public static MatchScore? Of(IEnumerable<FixtureMatch> matches)
    {
        var outcomes = matches.Where(m => m.Outcome is not null).ToList();
        if (outcomes.Count != MatchSlots.All.Length)
            return null;

        var won = outcomes.Count(m => m.Outcome == MatchOutcomes.Won);
        return new MatchScore(won, outcomes.Count - won);
    }
}
