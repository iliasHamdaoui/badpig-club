using BadPig.Shared.Core;

namespace BadPig.Announcements.Features.ListAnnouncementComments;

/// <summary>Comments of an announcement, as one conversation: search in the text or the author name, sort (createdAt; default oldest first) and pagination.</summary>
public sealed record ListAnnouncementCommentsRequest : ListQuery
{
    /// <summary>Identifier of the announcement (taken from the route).</summary>
    public string Id { get; init; } = "";
}
