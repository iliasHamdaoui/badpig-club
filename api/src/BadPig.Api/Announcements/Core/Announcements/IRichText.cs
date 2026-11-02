namespace BadPig.Announcements.Core;

/// <summary>Cleaning of the rich text written by the board: the announcements can be public, so no script or unsafe markup may be kept.</summary>
public interface IRichText
{
    /// <summary>The HTML with only the allowed tags (paragraphs, titles, lists, emphasis, links...) and safe links.</summary>
    string Sanitize(string html);

    /// <summary>Plain text excerpt (at most <paramref name="maxLength"/> characters) of the HTML.</summary>
    string ToSummary(string html, int maxLength);
}
