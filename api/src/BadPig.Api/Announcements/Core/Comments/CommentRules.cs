using BadPig.Shared.Core;

namespace BadPig.Announcements.Core;

/// <summary>Rules shared by the features that write or delete comments.</summary>
public static class CommentRules
{
    public const int MaxLength = 1000;

    /// <summary>A comment is deleted by its author, or by any board member.</summary>
    public static bool CanDelete(AnnouncementComment comment, IUserContext user) =>
        IsAuthor(comment, user) || user.IsInGroup(ClubGroup.Board);

    public static bool IsAuthor(AnnouncementComment comment, IUserContext user) => comment.AuthorId.ToString() == user.AccountId;
}
