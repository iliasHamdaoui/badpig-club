namespace BadPig.Announcements.Core;

/// <summary>Who can read an announcement.</summary>
public static class AnnouncementKinds
{
    /// <summary>Club members only.</summary>
    public const string Internal = "internal";

    /// <summary>Any visitor of the site.</summary>
    public const string Public = "public";

    public static bool IsValid(string? kind) => kind is Internal or Public;
}
