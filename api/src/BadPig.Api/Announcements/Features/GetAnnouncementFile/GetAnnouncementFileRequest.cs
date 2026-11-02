namespace BadPig.Announcements.Features.GetAnnouncementFile;

/// <summary>Download of the poster or of an attachment of an announcement.</summary>
/// <param name="Id">Identifier of the announcement.</param>
/// <param name="FileId">Identifier of the file (listed in the announcement).</param>
public sealed record GetAnnouncementFileRequest(string Id, string FileId);
