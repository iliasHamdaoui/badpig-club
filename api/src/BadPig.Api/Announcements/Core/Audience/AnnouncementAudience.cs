namespace BadPig.Announcements.Core;

/// <summary>What the person who reads the announcements is allowed to see.</summary>
public enum AnnouncementAudience
{
    /// <summary>Not signed in, or not a member with an up-to-date membership: public announcements only.</summary>
    Visitor,

    /// <summary>A club member with an up-to-date membership: published announcements, internal and public.</summary>
    Member,

    /// <summary>Board or Admin: everything, drafts and authors included.</summary>
    Board,
}
