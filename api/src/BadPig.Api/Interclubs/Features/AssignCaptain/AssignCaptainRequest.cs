namespace BadPig.Interclubs.Features.AssignCaptain;

/// <summary>Designation of a captain.</summary>
/// <param name="Id">Identifier of the team.</param>
/// <param name="Body">The account of the captain.</param>
public sealed record AssignCaptainRequest(string Id, AssignCaptainBody Body);
