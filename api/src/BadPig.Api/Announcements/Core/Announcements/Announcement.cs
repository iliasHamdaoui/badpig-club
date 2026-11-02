namespace BadPig.Announcements.Core;

/// <summary>
/// An announcement of the board. Internal = visible to the club members only; public = visible to any visitor.
/// A new announcement is not published. The author is only shown to the board.
/// </summary>
public sealed class Announcement
{
    public Guid Id { get; set; }
    public required string Title { get; set; }

    /// <summary>Rich text, already cleaned by <see cref="IRichText"/> (only safe tags and attributes).</summary>
    public required string BodyHtml { get; set; }

    /// <summary>Plain text excerpt shown in the lists.</summary>
    public string Summary { get; set; } = "";

    /// <summary>One of <see cref="AnnouncementKinds"/>.</summary>
    public required string Kind { get; set; }

    /// <summary>One of <see cref="AnnouncementForms"/>: a text, or a poll (which has its poll data and may have no text).</summary>
    public string Form { get; set; } = AnnouncementForms.Text;

    /// <summary>True when members can comment it (existing comments stay in the database when switched off).</summary>
    public bool CommentsEnabled { get; set; } = true;

    public bool IsPublished { get; set; }
    public bool IsPinned { get; set; }

    /// <summary>Null once the account of the author is deleted (right to erasure).</summary>
    public Guid? AuthorId { get; set; }
    public required string AuthorName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Date of the publication (announcements are listed from the most recent); null while not published.</summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>Last day the announcement is shown (Europe/Paris date); after it, it is hidden and then deleted automatically.</summary>
    public DateOnly ExpiresOn { get; set; }

    public List<AnnouncementFile> Files { get; set; } = [];
}
