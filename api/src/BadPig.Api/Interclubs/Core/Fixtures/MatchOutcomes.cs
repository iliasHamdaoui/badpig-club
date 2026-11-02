namespace BadPig.Interclubs.Core;

/// <summary>How a match ended, from the point of view of the club.</summary>
public static class MatchOutcomes
{
    public const string Won = "won";
    public const string Lost = "lost";

    public static readonly string[] All = [Won, Lost];
}
