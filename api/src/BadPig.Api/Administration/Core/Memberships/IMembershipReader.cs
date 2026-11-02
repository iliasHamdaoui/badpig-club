namespace BadPig.Administration.Core;

/// <summary>Read access to the memberships, owned by the Administration module.</summary>
public interface IMembershipReader
{
    Task<MembershipStatus> GetAsync(string accountId, CancellationToken ct);

    /// <summary>
    /// The membership of every person who has one, by account identifier. For the callers that need to sort a whole
    /// list of people (who may play in the interclubs, for instance) without asking season by season.
    /// </summary>
    Task<IReadOnlyDictionary<string, MembershipStatus>> GetAllAsync(CancellationToken ct);
}
