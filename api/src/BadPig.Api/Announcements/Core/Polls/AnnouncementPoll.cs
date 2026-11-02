namespace BadPig.Announcements.Core;

/// <summary>The poll of an announcement (at most one): only club members answer, until the closing date.</summary>
public sealed class AnnouncementPoll
{
    public Guid AnnouncementId { get; set; }

    /// <summary>True when a member can tick several options; false = exactly one.</summary>
    public bool AllowMultiple { get; set; }

    /// <summary>True when nobody (the board included) is shown who answered what.</summary>
    public bool IsAnonymous { get; set; }

    /// <summary>Last day the answers can be given or changed (Europe/Paris date).</summary>
    public DateOnly ClosesOn { get; set; }

    public List<PollOption> Options { get; set; } = [];
}
