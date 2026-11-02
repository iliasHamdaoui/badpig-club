namespace BadPig.Administration.Features.ListClubSessions;

/// <summary>How many next sessions the caller wants (1 to 10, 3 by default).</summary>
/// <param name="Count">Number of sessions to come to return.</param>
public sealed record ListClubSessionsRequest(int? Count);
