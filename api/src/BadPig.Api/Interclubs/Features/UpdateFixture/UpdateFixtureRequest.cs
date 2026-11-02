namespace BadPig.Interclubs.Features.UpdateFixture;

/// <summary>Change of a fixture.</summary>
/// <param name="Id">Identifier of the fixture.</param>
/// <param name="Body">The new values.</param>
public sealed record UpdateFixtureRequest(string Id, UpdateFixtureBody Body);
