namespace BadPig.Announcements.Core;

/// <summary>Which files can be attached to an announcement. The type is decided from the extension (the type sent by the browser is not trusted).</summary>
public static class AnnouncementFileRules
{
    public const long MaxPosterBytes = 5 * 1024 * 1024;
    public const long MaxAttachmentBytes = 10 * 1024 * 1024;
    public const int MaxAttachments = 10;

    /// <summary>Size limit of a whole upload request (file plus form overhead).</summary>
    public const long MaxRequestBytes = 12 * 1024 * 1024;

    private static readonly Dictionary<string, string> ImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
    };

    private static readonly Dictionary<string, string> DocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".txt"] = "text/plain",
        [".csv"] = "text/csv",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xls"] = "application/vnd.ms-excel",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".ppt"] = "application/vnd.ms-powerpoint",
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        [".odt"] = "application/vnd.oasis.opendocument.text",
        [".ods"] = "application/vnd.oasis.opendocument.spreadsheet",
        [".odp"] = "application/vnd.oasis.opendocument.presentation",
    };

    public static bool IsImage(string fileName) => ImageTypes.ContainsKey(Path.GetExtension(fileName));

    public static bool IsAllowedAttachment(string fileName) => IsImage(fileName) || DocumentTypes.ContainsKey(Path.GetExtension(fileName));

    /// <summary>Content type from the extension, or null if the extension is not allowed.</summary>
    public static string? ContentTypeOf(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return ImageTypes.GetValueOrDefault(extension) ?? DocumentTypes.GetValueOrDefault(extension);
    }

    /// <summary>The file name to keep: no folder, no control character, at most 255 characters.</summary>
    public static string CleanName(string fileName)
    {
        var name = new string(Path.GetFileName(fileName.Replace('\\', '/')).Where(c => !char.IsControl(c)).ToArray()).Trim();
        return name.Length <= 255 ? name : name[..255];
    }

    /// <summary>True if the first bytes of an image are those of its announced format (PNG, JPEG, GIF, WebP).</summary>
    // Raw bytes: a "\x89" in a UTF-8 string literal would be encoded as two bytes.
    private static ReadOnlySpan<byte> PngSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static bool HasImageSignature(ReadOnlySpan<byte> header) =>
        header.StartsWith(PngSignature)
        || header.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF })
        || header.StartsWith("GIF87a"u8)
        || header.StartsWith("GIF89a"u8)
        || (header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header.Slice(8, 4).SequenceEqual("WEBP"u8));
}
