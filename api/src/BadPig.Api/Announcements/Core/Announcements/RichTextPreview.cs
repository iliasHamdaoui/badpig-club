namespace BadPig.Announcements.Core;

/// <summary>The beginning of a rich text, still formatted, for the cards of the lists.</summary>
/// <param name="Html">The beginning of the text as safe HTML, without links (the card itself is a link).</param>
/// <param name="IsTruncated">True when the text goes on after the preview.</param>
public sealed record RichTextPreview(string Html, bool IsTruncated);
