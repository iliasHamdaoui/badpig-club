namespace BadPig.Announcements.Features.GetAnnouncement;

/// <summary>The poll of an announcement, as the reader may see it.</summary>
/// <param name="AllowMultiple">True when several options can be ticked; false = exactly one.</param>
/// <param name="IsAnonymous">True when nobody is shown who answered what.</param>
/// <param name="ClosesOn">Last day the answers can be given or changed (yyyy-MM-dd).</param>
/// <param name="IsClosed">True once the closing date has passed: nothing can change and the results are shown to the members (the board sees them at any time).</param>
/// <param name="ResultsVisible">True when the counts (and, for the board on a non-anonymous poll, the names) are filled: once the poll is closed for club members and the board, and at any time for the board.</param>
/// <param name="CanVote">True when the signed-in club member can answer or change their answer now (poll open and announcement published).</param>
/// <param name="ParticipantCount">Number of members who answered; null for a visitor.</param>
/// <param name="MyOptionIds">Identifiers of the options the signed-in member chose (empty if none, and for a visitor).</param>
/// <param name="Options">The choices, in order.</param>
public sealed record GetAnnouncementPoll(
    bool AllowMultiple,
    bool IsAnonymous,
    DateOnly ClosesOn,
    bool IsClosed,
    bool ResultsVisible,
    bool CanVote,
    int? ParticipantCount,
    IReadOnlyList<string> MyOptionIds,
    IReadOnlyList<GetAnnouncementPollOption> Options);
