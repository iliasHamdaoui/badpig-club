namespace BadPig.Interclubs.Features.SetMatchResult;

/// <summary>The points of one set.</summary>
/// <param name="Club">Points of the club (0 to 21).</param>
/// <param name="Opponent">Points of the opponent (0 to 21).</param>
public sealed record SetMatchResultSet(int? Club, int? Opponent);
