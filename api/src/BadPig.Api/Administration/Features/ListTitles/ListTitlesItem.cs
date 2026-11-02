namespace BadPig.Administration.Features.ListTitles;

/// <summary>One board title of the list, with its holder.</summary>
/// <param name="Id">Title identifier.</param>
/// <param name="Label">Name of the title (for example Président).</param>
/// <param name="HolderId">Account identifier of the person holding the title, or null.</param>
/// <param name="HolderName">Name of the person holding the title, or null.</param>
public sealed record ListTitlesItem(string Id, string Label, string? HolderId, string? HolderName);
