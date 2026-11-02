namespace BadPig.Announcements.Features.GetAnnouncement;

/// <summary>One choice of a poll.</summary>
/// <param name="Id">Option identifier.</param>
/// <param name="Label">Text of the choice.</param>
/// <param name="VoteCount">Number of members who chose it; only filled once the poll is closed, for club members (null before, and for a visitor).</param>
/// <param name="Voters">Names of the members who chose it; only filled once the poll is closed, for the board, when the poll is not anonymous (null otherwise).</param>
public sealed record GetAnnouncementPollOption(string Id, string Label, int? VoteCount, IReadOnlyList<string>? Voters);
