namespace BadPig.Announcements.Core;

/// <summary>Where the content of the posters and attachments is kept (a folder of the server's volume).</summary>
public interface IAnnouncementFileStore
{
    /// <summary>Stores the content and returns the key to read or delete it later.</summary>
    Task<string> SaveAsync(Stream content, CancellationToken ct);

    /// <summary>The content to read, or null if there is none under this key.</summary>
    Stream? Open(string key);

    /// <summary>Deletes the content (nothing happens if it is already gone).</summary>
    Task DeleteAsync(string key, CancellationToken ct);
}
