namespace BadPig.Administration.Core;

/// <summary>Read access to the board title of a person (used by the profile).</summary>
public interface IBoardInfo
{
    Task<BoardTitleInfo?> GetTitleAsync(string accountId, CancellationToken ct);
}
