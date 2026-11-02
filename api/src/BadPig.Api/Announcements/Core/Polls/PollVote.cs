namespace BadPig.Announcements.Core;

/// <summary>A member chose an option (a member who ticks two options of a poll has two rows).</summary>
public sealed class PollVote
{
    public Guid OptionId { get; set; }
    public Guid AnnouncementId { get; set; }
    public Guid MemberId { get; set; }
    public DateTimeOffset VotedAt { get; set; }
}
