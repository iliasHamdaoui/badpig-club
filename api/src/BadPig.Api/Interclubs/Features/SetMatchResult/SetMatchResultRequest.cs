namespace BadPig.Interclubs.Features.SetMatchResult;

/// <summary>Result of one match of a fixture.</summary>
/// <param name="Id">Identifier of the fixture.</param>
/// <param name="Slot">The match in the fixture (SD, MX1, MX2, DD, DH, SH1 or SH2).</param>
/// <param name="Body">How the match ended.</param>
public sealed record SetMatchResultRequest(string Id, string Slot, SetMatchResultBody Body);
