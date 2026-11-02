using System.Net;
using System.Text.RegularExpressions;
using BadPig.Announcements.Core;
using Ganss.Xss;

namespace BadPig.Announcements.Infrastructure;

/// <summary><see cref="IRichText"/> with the HtmlSanitizer library: an allow-list of tags and attributes, safe link schemes only.</summary>
internal sealed partial class HtmlRichText : IRichText
{
    private static readonly string[] Tags =
        ["p", "div", "br", "hr", "h2", "h3", "h4", "strong", "b", "em", "i", "u", "s", "sub", "sup", "ul", "ol", "li", "blockquote", "code", "pre", "a", "span"];

    public string Sanitize(string html)
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        foreach (var tag in Tags)
            sanitizer.AllowedTags.Add(tag);

        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedSchemes.Clear();
        foreach (var scheme in new[] { "https", "http", "mailto", "tel" })
            sanitizer.AllowedSchemes.Add(scheme);

        return sanitizer.Sanitize(html);
    }

    public string ToSummary(string html, int maxLength)
    {
        // Paragraph ends and line breaks become spaces, then every tag is removed.
        var spaced = BlockEnd().Replace(html, " ");
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedAttributes.Clear();
        sanitizer.KeepChildNodes = true; // by default a removed tag takes its text with it: here only the tags must go
        var text = WebUtility.HtmlDecode(sanitizer.Sanitize(spaced));
        text = Whitespace().Replace(text, " ").Trim();
        return text.Length <= maxLength ? text : text[..(maxLength - 1)].TrimEnd() + "…";
    }

    [GeneratedRegex(@"</(p|li|h[2-4]|blockquote|pre)>|<br\s*/?>|<hr\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockEnd();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
