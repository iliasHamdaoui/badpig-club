namespace BadPig.Announcements.Core;

/// <summary>The poster or an attachment of an announcement. The content is in the file store, under <see cref="StorageKey"/>.</summary>
public sealed class AnnouncementFile
{
    public Guid Id { get; set; }
    public Guid AnnouncementId { get; set; }

    /// <summary>One of <see cref="AnnouncementFileRoles"/>.</summary>
    public required string Role { get; set; }

    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long SizeBytes { get; set; }
    public required string StorageKey { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
