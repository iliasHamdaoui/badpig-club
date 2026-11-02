namespace BadPig.Announcements.Features.AddAnnouncementComment;

/// <summary>
/// Texts (in French, read by the club members) of the e-mails sent when a comment is written: to the author of the announcement,
/// and to the author of the comment that is answered. The greeting and the signature are added by the e-mail sender.
/// </summary>
internal static class CommentNotifications
{
    /// <summary>A new comment (or an answer to someone else) under an announcement or a poll of the recipient.</summary>
    public static (string Subject, string Body) OnYourAnnouncement(string commenter, string title, bool isPoll, string text, string? link) =>
        ($"Nouveau commentaire sur « {OneLine(title)} »",
            $"{commenter} a commenté {(isPoll ? "votre sondage" : "votre annonce")} « {title} » :\n\n{text}{LinkLine(link)}");

    /// <summary>An answer to a comment of the recipient.</summary>
    public static (string Subject, string Body) OnYourComment(string commenter, string title, string text, string quoted, string? link) =>
        ($"{OneLine(commenter)} a répondu à votre commentaire",
            $"{commenter} a répondu à votre commentaire sous « {title} » :\n\n{text}\n\nVotre commentaire : {quoted}{LinkLine(link)}");

    private static string LinkLine(string? link) => link is null ? "" : $"\n\nVoir la discussion : {link}";

    // A subject is a single line.
    private static string OneLine(string text) => text.Replace('\r', ' ').Replace('\n', ' ');
}
