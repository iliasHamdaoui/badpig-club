using BadPig.Shared.Core;

namespace BadPig.Announcements.Core;

/// <summary>The cleaned content of an announcement: safe HTML and its plain text excerpt.</summary>
/// <param name="Html">HTML with only the allowed tags.</param>
/// <param name="Summary">Plain text excerpt for the lists.</param>
public sealed record CleanedContent(string Html, string Summary);

/// <summary>Cleaning of the text written by the board (shared by the creation and the update).</summary>
public static class AnnouncementContent
{
    public const int SummaryLength = 280;

    public static Result<CleanedContent> Clean(IRichText richText, string html)
    {
        var cleaned = richText.Sanitize(html);
        var summary = richText.ToSummary(cleaned, SummaryLength);
        if (string.IsNullOrWhiteSpace(summary))
            return Error.Validation("bodyHtml", "Le contenu est vide : il ne reste aucun texte une fois nettoyé.");

        return new CleanedContent(cleaned, summary);
    }
}
