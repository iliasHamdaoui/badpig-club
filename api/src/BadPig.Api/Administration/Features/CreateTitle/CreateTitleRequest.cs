namespace BadPig.Administration.Features.CreateTitle;

/// <summary>JSON body of the request.</summary>
/// <param name="Label">Name of the new title (1 to 60 characters).</param>
public sealed record CreateTitleRequest(string? Label);
