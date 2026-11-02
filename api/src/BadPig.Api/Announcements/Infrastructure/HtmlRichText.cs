using System.Net;
using System.Text.RegularExpressions;
using BadPig.Announcements.Core;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
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

    public RichTextPreview ToPreview(string html, int maxLength)
    {
        var document = new HtmlParser().ParseDocument($"<body>{html}</body>");
        var body = document.Body!;

        // The card is a link itself: a link inside it would be invalid, so only its text is kept.
        foreach (var link in body.QuerySelectorAll("a").ToList())
            link.Replace(document.CreateTextNode(link.TextContent));

        var used = 0;
        var truncated = false;
        foreach (var node in body.ChildNodes.ToList())
        {
            if (truncated)
            {
                body.RemoveChild(node);
                continue;
            }

            var length = Weight(node);
            if (used + length <= maxLength)
            {
                used += length;
                continue;
            }

            // The first block is kept even when it is long on its own (the card clips it); later ones are cut between two blocks.
            truncated = true;
            if (node is IElement { LocalName: "ul" or "ol" } list)
                KeepItemsWithin(list, maxLength - used, nothingKeptYet: used == 0);
            else if (used > 0)
                body.RemoveChild(node);
        }

        return new RichTextPreview(body.InnerHtml.Trim(), truncated);
    }

    private static void KeepItemsWithin(IElement list, int budget, bool nothingKeptYet)
    {
        var kept = 0;
        var used = 0;
        foreach (var item in list.Children.ToList())
        {
            used += Weight(item);
            if (used > budget && (kept > 0 || !nothingKeptYet))
                list.RemoveChild(item);
            else
                kept++;
        }

        if (kept == 0)
            list.Remove();
    }

    /// <summary>
    /// What a block costs in the preview: its text plus a fixed amount per block (or per list item), so that many short lines
    /// are cut as well as a few long paragraphs.
    /// </summary>
    private static int Weight(INode node)
    {
        if (node is IElement { LocalName: "ul" or "ol" } list)
            return list.Children.Sum(item => Weight(item));

        var text = Whitespace().Replace(node.TextContent, " ").Trim().Length;
        return text == 0 ? 0 : text + BlockWeight;
    }

    private const int BlockWeight = 60;

    [GeneratedRegex(@"</(p|li|h[2-4]|blockquote|pre)>|<br\s*/?>|<hr\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockEnd();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
