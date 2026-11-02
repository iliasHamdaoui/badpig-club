namespace BadPig.Announcements.Features.VoteInPoll;

/// <summary>JSON body of an answer to a poll.</summary>
/// <param name="OptionIds">Identifiers of the chosen options: exactly one when the poll accepts a single answer, one or more otherwise. They replace the previous answer.</param>
public sealed record VoteInPollBody(string[]? OptionIds);
