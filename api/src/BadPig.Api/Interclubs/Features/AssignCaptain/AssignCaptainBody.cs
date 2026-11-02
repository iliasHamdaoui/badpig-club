namespace BadPig.Interclubs.Features.AssignCaptain;

/// <summary>JSON body of the designation of a captain.</summary>
/// <param name="CaptainId">Account identifier of the captain (an active member of the club).</param>
public sealed record AssignCaptainBody(string? CaptainId);
