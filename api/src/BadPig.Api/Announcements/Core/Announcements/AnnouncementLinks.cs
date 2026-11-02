namespace BadPig.Announcements.Core;

/// <summary>Where the site lives, to put a link to an announcement in an e-mail (section "Announcements", setting SiteUrl).</summary>
public sealed class AnnouncementLinks
{
    public const string Section = "Announcements";

    /// <summary>Public address of the site, for instance https://test.badpig.fr. Empty: the e-mails carry no link.</summary>
    public string SiteUrl { get; set; } = "";

    /// <summary>The address of the page of an announcement; null when the site address is not configured.</summary>
    public string? PageOf(Guid announcementId) =>
        string.IsNullOrWhiteSpace(SiteUrl) ? null : $"{SiteUrl.Trim().TrimEnd('/')}/announcements/{announcementId}";
}
