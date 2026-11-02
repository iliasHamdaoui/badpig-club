namespace BadPig.Interclubs.Features.SetFixtureResult;

/// <summary>The sets of one match.</summary>
/// <param name="Slot">SD, MX1, MX2, DD, DH, SH1 or SH2.</param>
/// <param name="Ending">played (the default), forfeit or retirement.</param>
/// <param name="Withdrawn">club or opponent: the side that gave up; required for a forfeit or a retirement, forbidden for a played match.</param>
/// <param name="Sets">Played: two or three sets, in order (15 points with a lead of 2; from 14-14 a lead of 2 or 21; the match ends as soon as a side has won 2 sets). Retirement: the sets completed so far (none, or one, or two that do not decide the match). Forfeit: none.</param>
public sealed record SetFixtureResultMatch(string? Slot, string? Ending, string? Withdrawn, List<SetFixtureResultSet>? Sets);
