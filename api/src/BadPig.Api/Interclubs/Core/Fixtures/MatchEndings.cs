namespace BadPig.Interclubs.Core;

/// <summary>How a match ended.</summary>
public static class MatchEndings
{
    /// <summary>Played to the end: 2 or 3 sets, the outcome comes from the sets.</summary>
    public const string Played = "played";

    /// <summary>Not played (forfeit, "w.o."): the other side wins, there are no sets.</summary>
    public const string Forfeit = "forfeit";

    /// <summary>Stopped during the play (injury...): the other side wins; the sets completed so far may be kept.</summary>
    public const string Retirement = "retirement";

    public static readonly string[] All = [Played, Forfeit, Retirement];
}
