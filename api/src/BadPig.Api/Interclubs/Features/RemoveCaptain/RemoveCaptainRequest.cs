namespace BadPig.Interclubs.Features.RemoveCaptain;

/// <summary>Removal of the captain of a team.</summary>
/// <param name="Id">Identifier of the team.</param>
public sealed record RemoveCaptainRequest(string Id);
