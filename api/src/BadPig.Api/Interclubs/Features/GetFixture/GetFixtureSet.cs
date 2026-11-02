namespace BadPig.Interclubs.Features.GetFixture;

/// <summary>The points of one set.</summary>
/// <param name="Club">Points of the club (15 when it won the set).</param>
/// <param name="Opponent">Points of the opponent (15 when it won the set).</param>
public sealed record GetFixtureSet(int Club, int Opponent);
