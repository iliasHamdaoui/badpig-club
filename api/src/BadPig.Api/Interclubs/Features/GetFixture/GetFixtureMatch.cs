namespace BadPig.Interclubs.Features.GetFixture;

/// <summary>One of the seven matches of a fixture.</summary>
/// <param name="Slot">SD (women's singles), MX1, MX2 (mixed doubles), DD (women's doubles), DH (men's doubles), SH1, SH2 (men's singles).</param>
/// <param name="Players">The players chosen by the captain (one for a singles match, two otherwise); empty until the lineup is set.</param>
/// <param name="Sets">The sets played, in order (two or three); empty until the result is entered.</param>
/// <param name="Ending">played, forfeit (not played, awarded to the other side) or retirement (stopped during the play); null until the result is entered.</param>
/// <param name="Withdrawn">club or opponent: the side that gave up, for a forfeit or a retirement; null otherwise.</param>
/// <param name="Outcome">won or lost, from the point of view of the club, computed from the sets (2 sets win the match); null until the result is entered.</param>
public sealed record GetFixtureMatch(string Slot, IReadOnlyList<GetFixturePlayer> Players, IReadOnlyList<GetFixtureSet> Sets, string? Ending, string? Withdrawn, string? Outcome);
