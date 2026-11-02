namespace BadPig.Announcements.Core;

/// <summary>One choice of a poll.</summary>
public sealed class PollOption
{
    public Guid Id { get; set; }
    public Guid AnnouncementId { get; set; }
    public required string Label { get; set; }

    /// <summary>Order of display (0 first).</summary>
    public int Position { get; set; }
}
