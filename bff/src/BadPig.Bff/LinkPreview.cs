using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BadPig.Bff;

/// <summary>
/// Link previews: when a messaging app (WhatsApp, Telegram, Facebook...) or a search engine reads the address of a public announcement,
/// it does not run the site's scripts, so it would only see the generic page. For these robots only, the BFF returns the site's page
/// with the title, an excerpt and the poster of the announcement in its head (Open Graph tags). Only what any visitor can already read
/// is used: the API is called without a token, so internal announcements and drafts keep the generic page.
/// </summary>
internal static partial class LinkPreview
{
    public const string HttpClientName = "link-preview";

    private const int DescriptionLength = 200;

    [GeneratedRegex(@"^/announcements/(?<id>[0-9a-fA-F]{8}(-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12})/?$")]
    private static partial Regex AnnouncementPath();

    [GeneratedRegex(@"WhatsApp|facebookexternalhit|Facebot|Twitterbot|TelegramBot|Slackbot|LinkedInBot|Discordbot|SkypeUriPreview|Signal|Googlebot|bingbot|Applebot|Mastodon", RegexOptions.IgnoreCase)]
    private static partial Regex Robot();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex Tags();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();

    /// <summary>The identifier when the request is a robot reading the address of an announcement; null otherwise.</summary>
    public static string? AnnouncementIdFor(HttpRequest request) =>
        HttpMethods.IsGet(request.Method) && Robot().IsMatch(request.Headers.UserAgent.ToString()) && AnnouncementPath().Match(request.Path.Value ?? "") is { Success: true } m
            ? m.Groups["id"].Value
            : null;

    /// <summary>The site's page with the preview tags of the public announcement; null when the announcement cannot be read by a visitor.</summary>
    public static async Task<string?> RenderAsync(HttpClient api, string indexHtml, string siteUrl, string id, CancellationToken ct)
    {
        try
        {
            using var response = await api.GetAsync($"api/v1/announcements/{id}", ct);
            if (!response.IsSuccessStatusCode)
                return null;

            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = json.RootElement;
            var title = root.GetProperty("title").GetString() ?? "";
            var description = Excerpt(root.GetProperty("bodyHtml").GetString() ?? "");
            if (description.Length == 0 && root.TryGetProperty("form", out var form) && form.GetString() == "poll")
                description = "Sondage du club : donnez votre avis.";

            var posterId = PosterId(root);
            return Inject(indexHtml, title, description, $"{siteUrl}/announcements/{id}", posterId is null ? null : $"{siteUrl}/api/v1/announcements/{id}/files/{posterId}");
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            return null; // the generic page is returned instead
        }
    }

    /// <summary>The text of the announcement without markup, on one line and shortened.</summary>
    public static string Excerpt(string html)
    {
        var text = WebUtility.HtmlDecode(Tags().Replace(html.Replace("</p>", " ").Replace("<br>", " "), " "));
        text = Spaces().Replace(text, " ").Trim();
        return text.Length <= DescriptionLength ? text : text[..DescriptionLength].TrimEnd() + "…";
    }

    private static string? PosterId(JsonElement announcement)
    {
        if (!announcement.TryGetProperty("files", out var files))
            return null;

        foreach (var file in files.EnumerateArray())
        {
            if (file.TryGetProperty("isPoster", out var isPoster) && isPoster.GetBoolean())
                return file.GetProperty("id").GetString();
        }

        return null;
    }

    private static string Inject(string indexHtml, string title, string description, string url, string? imageUrl)
    {
        var fullTitle = $"{title} - BadPig";
        var tags = new List<string>
        {
            Meta("property", "og:type", "article"),
            Meta("property", "og:site_name", "BadPig - Badminton Pignan"),
            Meta("property", "og:locale", "fr_FR"),
            Meta("property", "og:title", title),
            Meta("property", "og:url", url),
            Meta("name", "description", description),
            Meta("property", "og:description", description),
            Meta("name", "twitter:card", imageUrl is null ? "summary" : "summary_large_image"),
        };
        if (imageUrl is not null)
            tags.Add(Meta("property", "og:image", imageUrl));

        var newTitle = $"<title>{WebUtility.HtmlEncode(fullTitle)}</title>";
        var withTitle = Regex.Replace(indexHtml, "<title>.*?</title>", _ => newTitle, RegexOptions.Singleline);
        return withTitle.Replace("</head>", string.Join("\n  ", tags) + "\n</head>");
    }

    private static string Meta(string attribute, string name, string content) =>
        $"<meta {attribute}=\"{name}\" content=\"{WebUtility.HtmlEncode(content)}\">";
}
