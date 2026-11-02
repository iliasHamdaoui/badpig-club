namespace BadPig.Interclubs.Features.CreateFixture;

/// <summary>Creation of a fixture.</summary>
/// <param name="TeamId">Identifier of the team.</param>
/// <param name="Body">The fixture.</param>
public sealed record CreateFixtureRequest(string TeamId, CreateFixtureBody Body);
