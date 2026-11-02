namespace BadPig.Interclubs.Features.SetLineup;

/// <summary>Lineup of a fixture.</summary>
/// <param name="Id">Identifier of the fixture.</param>
/// <param name="Body">The players of each match.</param>
public sealed record SetLineupRequest(string Id, SetLineupBody Body);
