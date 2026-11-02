namespace BadPig.Administration.Features.RenameTitle;

/// <summary>JSON body of the request (the title id comes from the route).</summary>
/// <param name="Label">New name (1 to 60 characters).</param>
public sealed record RenameTitleBody(string? Label);
