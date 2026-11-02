namespace BadPig.Administration.Features.DeleteTitle;

/// <param name="Id">Title identifier (from the route).</param>
public sealed record DeleteTitleRequest(string Id);
