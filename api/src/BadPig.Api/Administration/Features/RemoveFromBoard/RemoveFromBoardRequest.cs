namespace BadPig.Administration.Features.RemoveFromBoard;

/// <param name="Id">account identifier of the person (from the route).</param>
public sealed record RemoveFromBoardRequest(string Id);
