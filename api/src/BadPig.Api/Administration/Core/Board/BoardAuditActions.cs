namespace BadPig.Administration.Core;

/// <summary>
/// Audit action codes used by several features (the codes used by a single feature are defined in that feature).
/// Stored values: never change a value that is already in use.
/// </summary>
public static class BoardAuditActions
{
    /// <summary>A person joins the board (AddToBoard, InviteUser).</summary>
    public const string Joined = "board.joined";

    /// <summary>A person loses their title (SetTitle, DeleteTitle).</summary>
    public static string TitleRemoved(string label) => $"title.removed.{label}";
}
