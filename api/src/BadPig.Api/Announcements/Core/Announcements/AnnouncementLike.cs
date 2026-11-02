namespace BadPig.Announcements.Core;

/// <summary>A member likes an announcement (one like per member and announcement).</summary>
public sealed class AnnouncementLike
{
    public Guid AnnouncementId { get; set; }
    public Guid MemberId { get; set; }
    public DateTimeOffset LikedAt { get; set; }
}
