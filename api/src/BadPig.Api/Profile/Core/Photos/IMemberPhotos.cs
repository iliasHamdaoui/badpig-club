namespace BadPig.Profile.Core;

/// <summary>What the other modules may ask about the profile pictures (they never read the pictures themselves).</summary>
public interface IMemberPhotos
{
    /// <summary>
    /// The version of the picture (Unix time in milliseconds of its last change) of each of these accounts that has one.
    /// An account without a picture is absent from the result. The picture is served at <c>/members/{id}/photo?v={version}</c>.
    /// </summary>
    Task<IReadOnlyDictionary<string, long>> GetVersionsAsync(IReadOnlyCollection<string> accountIds, CancellationToken ct);
}
