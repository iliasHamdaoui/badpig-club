namespace BadPig.Interclubs.Features.SetFixtureResult;

/// <summary>JSON body of the result of a fixture.</summary>
/// <param name="Matches">The seven matches (SD, MX1, MX2, DD, DH, SH1, SH2), each once.</param>
public sealed record SetFixtureResultBody(List<SetFixtureResultMatch>? Matches);
