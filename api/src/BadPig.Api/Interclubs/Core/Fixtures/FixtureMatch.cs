namespace BadPig.Interclubs.Core;

/// <summary>One of the seven matches of a fixture: its players (chosen by the captain) and the sets once it is played.</summary>
public sealed class FixtureMatch
{
    public Guid FixtureId { get; set; }

    /// <summary>One of <see cref="MatchSlots"/>.</summary>
    public string Slot { get; set; } = "";

    public Guid? Player1Id { get; set; }

    /// <summary>The partner, for a doubles or mixed match.</summary>
    public Guid? Player2Id { get; set; }

    public int? Set1Club { get; set; }
    public int? Set1Opponent { get; set; }
    public int? Set2Club { get; set; }
    public int? Set2Opponent { get; set; }
    public int? Set3Club { get; set; }
    public int? Set3Opponent { get; set; }

    /// <summary>One of <see cref="MatchEndings"/>; null until the result is entered.</summary>
    public string? Ending { get; set; }

    /// <summary>One of <see cref="MatchSides"/>: the side that gave up, for a forfeit or a retirement.</summary>
    public string? Withdrawn { get; set; }

    public IEnumerable<Guid> Players => new[] { Player1Id, Player2Id }.OfType<Guid>();

    /// <summary>The sets played, in order (none until the result is entered).</summary>
    public IReadOnlyList<SetScore> Sets => new[] { (Set1Club, Set1Opponent), (Set2Club, Set2Opponent), (Set3Club, Set3Opponent) }
        .Where(s => s.Item1 is not null && s.Item2 is not null).Select(s => new SetScore(s.Item1!.Value, s.Item2!.Value)).ToList();

    /// <summary>One of <see cref="MatchOutcomes"/>: from the side that gave up after a forfeit or a retirement, otherwise computed from the sets; null until the match is decided.</summary>
    public string? Outcome => Ending is MatchEndings.Forfeit or MatchEndings.Retirement && Withdrawn is not null
        ? MatchRules.OutcomeOfWithdrawal(Withdrawn)
        : MatchRules.OutcomeOf(Sets);

    /// <summary>Records how the match ended, with the sets played (at most three).</summary>
    public void SetResult(string ending, string? withdrawn, IReadOnlyList<SetScore> sets)
    {
        Ending = ending;
        Withdrawn = withdrawn;
        (Set1Club, Set1Opponent) = At(sets, 0);
        (Set2Club, Set2Opponent) = At(sets, 1);
        (Set3Club, Set3Opponent) = At(sets, 2);
    }

    private static (int?, int?) At(IReadOnlyList<SetScore> sets, int index) =>
        index < sets.Count ? (sets[index].Club, sets[index].Opponent) : (null, null);
}
