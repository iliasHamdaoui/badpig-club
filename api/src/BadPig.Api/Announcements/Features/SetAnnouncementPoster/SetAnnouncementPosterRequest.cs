using BadPig.Announcements.Core;

namespace BadPig.Announcements.Features.SetAnnouncementPoster;

/// <summary>Sets the poster of an announcement.</summary>
/// <param name="Id">Identifier of the announcement.</param>
/// <param name="File">The image sent (JPEG, PNG, WebP or GIF, 5 MB at most).</param>
public sealed record SetAnnouncementPosterRequest(string Id, UploadedFile? File);
