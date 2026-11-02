namespace BadPig.Interclubs.Features.SetFixtureResult;

/// <summary>Result of a fixture.</summary>
/// <param name="Id">Identifier of the fixture.</param>
/// <param name="Body">The sets of the seven matches.</param>
public sealed record SetFixtureResultRequest(string Id, SetFixtureResultBody Body);
