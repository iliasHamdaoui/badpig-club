namespace BadPig.Administration.Features.RenameTitle;

/// <param name="Id">Title identifier (from the route).</param>
/// <param name="Label">New name (1 to 60 characters).</param>
public sealed record RenameTitleRequest(string Id, string? Label);
