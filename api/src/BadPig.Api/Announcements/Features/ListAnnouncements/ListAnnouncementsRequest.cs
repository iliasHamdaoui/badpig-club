using BadPig.Shared.Core;

namespace BadPig.Announcements.Features.ListAnnouncements;

/// <summary>Search (title and excerpt), filters, sort (publishedAt, createdAt, title; default publishedAt, most recent first) and pagination. Pinned announcements always come first.</summary>
public sealed record ListAnnouncementsRequest : ListQuery
{
    /// <summary>Only the announcements of this type: "internal" or "public".</summary>
    public string? Kind { get; init; }

    /// <summary>Only the announcements of this form: "text" or "poll".</summary>
    public string? Form { get; init; }

    /// <summary>Board only: "published" or "draft". Other readers only ever see published announcements.</summary>
    public string? Status { get; init; }
}
