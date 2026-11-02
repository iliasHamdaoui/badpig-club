
namespace BadPig.Administration.Features.SetTitle;

/// <summary>JSON body of the request (the account id comes from the route).</summary>
/// <param name="TitleId">Title to assign; empty or null removes the person's title.</param>
public sealed record SetTitleBody(string? TitleId);
