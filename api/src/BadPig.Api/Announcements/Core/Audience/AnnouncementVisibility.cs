namespace BadPig.Announcements.Core;

/// <summary>Which announcements an audience can read. Over announcements are only shown to the board (until they are deleted).</summary>
public static class AnnouncementVisibility
{
    public static IQueryable<Announcement> VisibleTo(this IQueryable<Announcement> announcements, AnnouncementAudience audience, DateOnly today) => audience switch
    {
        AnnouncementAudience.Board => announcements,
        AnnouncementAudience.Member => announcements.Where(a => a.IsPublished && a.ExpiresOn >= today),
        _ => announcements.Where(a => a.IsPublished && a.ExpiresOn >= today && a.Kind == AnnouncementKinds.Public),
    };
}
