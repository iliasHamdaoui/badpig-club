namespace BadPig.Administration.Features.SetTitle;

/// <param name="Id">account identifier of the board member (from the route).</param>
/// <param name="TitleId">Title to assign; empty or null removes the person's title.</param>
public sealed record SetTitleRequest(string Id, string? TitleId);
