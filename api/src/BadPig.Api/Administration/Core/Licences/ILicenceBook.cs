namespace BadPig.Administration.Core;

/// <summary>
/// The licence numbers of the club, owned by the Administration module: the Profile module reads and changes
/// the one of the signed-in person through this interface, never through the table.
/// </summary>
public interface ILicenceBook
{
    /// <summary>The licence number of an account; null when there is none.</summary>
    Task<string?> GetAsync(string accountId, CancellationToken ct);

    /// <summary>
    /// True when the number is already on another account's record (upper and lower case are the same number).
    /// An empty number is never taken: the club has as many people without a licence as it likes.
    /// </summary>
    Task<bool> IsTakenAsync(string? number, string exceptAccountId, CancellationToken ct);

    /// <summary>Records the licence number of an account; a null or empty number removes the one on record.</summary>
    Task SetAsync(string accountId, string? number, CancellationToken ct);
}
