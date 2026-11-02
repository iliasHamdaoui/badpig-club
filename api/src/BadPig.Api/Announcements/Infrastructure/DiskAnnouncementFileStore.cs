using BadPig.Announcements.Core;
using Microsoft.Extensions.Options;

namespace BadPig.Announcements.Infrastructure;

internal sealed class AnnouncementStorageOptions
{
    public const string Section = "Announcements";

    /// <summary>Folder (a volume of the container) where the files are written.</summary>
    public string StoragePath { get; set; } = "data/announcements";
}

/// <summary><see cref="IAnnouncementFileStore"/> on the disk of the server: one file per key (a random identifier, never the name sent by the person).</summary>
internal sealed class DiskAnnouncementFileStore(IOptions<AnnouncementStorageOptions> options) : IAnnouncementFileStore
{
    private string Folder => Path.GetFullPath(options.Value.StoragePath);

    public async Task<string> SaveAsync(Stream content, CancellationToken ct)
    {
        Directory.CreateDirectory(Folder);
        var key = Guid.NewGuid().ToString("N");
        await using var target = new FileStream(PathOf(key), FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(target, ct);
        return key;
    }

    public Stream? Open(string key)
    {
        var path = PathOf(key);
        return File.Exists(path) ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
    }

    public Task DeleteAsync(string key, CancellationToken ct)
    {
        File.Delete(PathOf(key));
        return Task.CompletedTask;
    }

    // Only keys made by SaveAsync are accepted: no way to reach another file with "../".
    private string PathOf(string key) =>
        Guid.TryParseExact(key, "N", out _) ? Path.Combine(Folder, key) : throw new ArgumentException("Invalid storage key.", nameof(key));
}
