namespace BadPig.Announcements.Core;

/// <summary>Cleaning of the rich text written by the board: the announcements can be public, so no script or unsafe markup may be kept.</summary>
public interface IRichText
{
    /// <summary>The HTML with only the allowed tags (paragraphs, titles, lists, emphasis, links...) and safe links.</summary>
    string Sanitize(string html);

    /// <summary>Plain text excerpt (at most <paramref name="maxLength"/> characters) of the HTML.</summary>
    string ToSummary(string html, int maxLength);

    /// <summary>
    /// The beginning of the HTML, with its titles, lists and emphasis, cut between two blocks once the budget <paramref name="maxLength"/> is reached
    /// (the characters of the text, plus a fixed cost per block or list item); links are replaced by their text.
    /// </summary>
    RichTextPreview ToPreview(string html, int maxLength);
}
