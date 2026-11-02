namespace BadPig.Announcements.Core;

/// <summary>A comment written by a club member under an announcement. Its author or any board member can delete it.</summary>
public sealed class AnnouncementComment
{
    public Guid Id { get; set; }
    public Guid AnnouncementId { get; set; }

    /// <summary>The comment this one quotes (answers to it, chat style); null for a message of its own. It may no longer exist: the quote then shows "message deleted".</summary>
    public Guid? ParentId { get; set; }

    public Guid AuthorId { get; set; }

    /// <summary>Display name of the author when the comment was written.</summary>
    public required string AuthorName { get; set; }

    /// <summary>Plain text (never HTML), 1 to <see cref="CommentRules.MaxLength"/> characters.</summary>
    public required string Text { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
