namespace BadPig.Announcements.Features.WithdrawPollVote;

/// <summary>A member withdraws their answer to a poll.</summary>
/// <param name="Id">Identifier of the announcement.</param>
public sealed record WithdrawPollVoteRequest(string Id);
