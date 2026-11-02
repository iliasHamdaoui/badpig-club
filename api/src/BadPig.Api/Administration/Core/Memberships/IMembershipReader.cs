namespace BadPig.Administration.Core;

/// <summary>Read access to the memberships, owned by the Administration module.</summary>
public interface IMembershipReader
{
    Task<MembershipStatus> GetAsync(string accountId, CancellationToken ct);
}
