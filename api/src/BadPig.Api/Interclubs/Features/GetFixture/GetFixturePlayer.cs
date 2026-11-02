namespace BadPig.Interclubs.Features.GetFixture;

/// <summary>A player of a match.</summary>
/// <param name="AccountId">Account identifier of the player.</param>
/// <param name="Name">Name of the player.</param>
public sealed record GetFixturePlayer(string AccountId, string Name);
