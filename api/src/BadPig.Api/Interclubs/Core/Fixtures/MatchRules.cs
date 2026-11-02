namespace BadPig.Interclubs.Core;

/// <summary>
/// How a match is played and who wins it. A set is won at 15 points with 2 points of lead; at 14-14 play goes on until one side leads by 2,
/// or until the first side reaches 21. A match has 3 sets at most and is won with 2 sets. A match can also end by forfeit or retirement.
/// The outcome of a match is never typed: it is computed from the sets, or from the side that gave up.
/// </summary>
public static class MatchRules
{
    public const int PointsToWinSet = 15;
    public const int MaxPointsInSet = 21;
    public const int SetsToWinMatch = 2;
    public const int MaxSets = 3;

    /// <summary>
    /// A finished set: 15 points against 13 or fewer; from 14-14, a lead of 2 (16-14, 17-15… 21-19); or 21-20 (the first side to 21 wins).
    /// </summary>
    public static bool IsValidSet(SetScore set)
    {
        var (winner, loser) = set.Club > set.Opponent ? (set.Club, set.Opponent) : (set.Opponent, set.Club);
        if (loser < 0 || winner == loser)
            return false;

        return IsWonAtFifteen(winner, loser) || IsWonOnTheLead(winner, loser) || IsWonAtTheCap(winner, loser);
    }

    private static bool IsWonAtFifteen(int winner, int loser) => winner == PointsToWinSet && loser <= PointsToWinSet - 2;

    private static bool IsWonOnTheLead(int winner, int loser) => winner > PointsToWinSet && winner < MaxPointsInSet && loser == winner - 2;

    private static bool IsWonAtTheCap(int winner, int loser) => winner == MaxPointsInSet && loser >= MaxPointsInSet - 2 && loser < MaxPointsInSet;

    /// <summary>Valid sets, and the match stops as soon as a side has won its second set (no set before that point decides it, none after it).</summary>
    public static bool IsValidMatch(IReadOnlyList<SetScore> sets) =>
        sets.Count is >= SetsToWinMatch and <= MaxSets
        && sets.All(IsValidSet)
        && OutcomeOf(sets) is not null
        && OutcomeOf(sets.Take(sets.Count - 1).ToList()) is null;

    /// <summary>
    /// What a captain may enter for a match: played (a finished match), forfeit (a side given, no sets) or retirement (a side given,
    /// and the sets completed so far, which do not decide the match).
    /// </summary>
    public static bool IsValidEntry(string ending, string? withdrawn, IReadOnlyList<SetScore> sets) => ending switch
    {
        MatchEndings.Played => withdrawn is null && IsValidMatch(sets),
        MatchEndings.Forfeit => IsSide(withdrawn) && sets.Count == 0,
        MatchEndings.Retirement => IsSide(withdrawn) && sets.Count < SetsToWinMatch + 1 && sets.All(IsValidSet) && OutcomeOf(sets) is null,
        _ => false,
    };

    private static bool IsSide(string? side) => side is not null && MatchSides.All.Contains(side);

    /// <summary>won or lost from the point of view of the club (<see cref="MatchOutcomes"/>) when played; null while no side has won 2 sets.</summary>
    public static string? OutcomeOf(IReadOnlyList<SetScore> sets)
    {
        var clubSets = sets.Count(s => s.Club > s.Opponent);
        var opponentSets = sets.Count(s => s.Opponent > s.Club);
        if (clubSets >= SetsToWinMatch)
            return MatchOutcomes.Won;

        return opponentSets >= SetsToWinMatch ? MatchOutcomes.Lost : null;
    }

    /// <summary>The side that gave up loses the match.</summary>
    public static string OutcomeOfWithdrawal(string withdrawn) => withdrawn == MatchSides.Club ? MatchOutcomes.Lost : MatchOutcomes.Won;
}
