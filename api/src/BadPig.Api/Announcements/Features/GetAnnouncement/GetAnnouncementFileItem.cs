namespace BadPig.Announcements.Features.GetAnnouncement;

/// <summary>The poster or an attachment of an announcement.</summary>
/// <param name="Id">File identifier, for the file route.</param>
/// <param name="FileName">Name of the file.</param>
/// <param name="ContentType">Media type of the file.</param>
/// <param name="SizeBytes">Size in bytes.</param>
/// <param name="IsPoster">True for the poster (an image to display), false for an attachment (to download).</param>
public sealed record GetAnnouncementFileItem(string Id, string FileName, string ContentType, long SizeBytes, bool IsPoster);
