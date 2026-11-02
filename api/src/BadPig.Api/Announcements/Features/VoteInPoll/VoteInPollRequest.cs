namespace BadPig.Announcements.Features.VoteInPoll;

/// <summary>A member answers the poll of an announcement (or changes the answer).</summary>
/// <param name="Id">Identifier of the announcement.</param>
/// <param name="Body">The chosen options.</param>
public sealed record VoteInPollRequest(string Id, VoteInPollBody Body);
