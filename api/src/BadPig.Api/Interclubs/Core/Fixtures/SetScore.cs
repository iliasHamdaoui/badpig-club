namespace BadPig.Interclubs.Core;

/// <summary>The points of one set: those of the club, then those of the opponent.</summary>
public readonly record struct SetScore(int Club, int Opponent);
