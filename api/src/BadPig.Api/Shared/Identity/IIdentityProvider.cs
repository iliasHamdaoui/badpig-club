using BadPig.Shared.Core;

namespace BadPig.Shared.Identity;

/// <summary>
/// The identity provider (IDP) that holds the club accounts. This is the ONLY contract the modules use to read or change
/// accounts, groups and sign-in state: they never know which product is behind it (today Authentik, see this folder).
/// Everything in this folder other than the types below is internal.
/// An account is identified by its stable identifier (<see cref="AccountInfo.Id"/>), never by its e-mail address.
/// </summary>
public interface IIdentityProvider
{
    Task<AccountInfo?> FindAsync(string accountId, CancellationToken ct);

    /// <summary>Finds an account by e-mail address (exact match, case-insensitive).</summary>
    Task<AccountInfo?> FindByEmailAsync(string email, CancellationToken ct);

    /// <summary>All people accounts (technical accounts included: filter with <see cref="AccountRules.IsClubMember"/>), ordered by name.</summary>
    Task<IReadOnlyList<AccountInfo>> ListAsync(CancellationToken ct);

    /// <summary>Creates an active account without a password, member of the given club groups. Throws <see cref="IdentityConfigurationException"/> if a group is not configured.</summary>
    Task<AccountInfo> CreateAsync(string name, string email, IReadOnlyCollection<ClubGroup> groups, CancellationToken ct);

    Task<AccountInfo> UpdateProfileAsync(string accountId, string name, string email, CancellationToken ct);

    /// <summary>Disables or re-enables an account (a disabled account cannot sign in).</summary>
    Task SetActiveAsync(string accountId, bool active, CancellationToken ct);

    /// <summary>Permanently deletes the account from the identity provider (the modules erase their own data separately).</summary>
    Task DeleteAsync(string accountId, CancellationToken ct);

    /// <summary>Adds the account to a club group. Throws <see cref="IdentityConfigurationException"/> if the group is not configured.</summary>
    Task AddToGroupAsync(string accountId, ClubGroup group, CancellationToken ct);

    Task RemoveFromGroupAsync(string accountId, ClubGroup group, CancellationToken ct);

    /// <summary>Sends the person an e-mail with a link (valid 3 days) to choose a password. Throws <see cref="IdentityConfigurationException"/> if invitations are not configured; any other exception means the e-mail was not sent.</summary>
    Task SendSignupLinkAsync(string accountId, CancellationToken ct);

    /// <summary>True when the invitation e-mail is configured (checked before creating an account for nothing).</summary>
    bool CanSendSignupLink { get; }
}

/// <summary>The identity provider is not fully configured (missing group or e-mail stage identifier).</summary>
public sealed class IdentityConfigurationException(string message) : InvalidOperationException(message);
