using BadPig.Shared.Identity;

namespace BadPig.Shared.Core;

/// <summary>
/// Permanent deletion of an account, shared by the three ways to delete one (by its owner, by the board, automatically):
/// the account is removed from the identity provider, then every module erases its own data (<see cref="IAccountErasure"/>).
/// </summary>
public sealed class AccountDeletionService(IIdentityProvider identity, IEnumerable<IAccountErasure> erasures)
{
    public async Task DeleteAsync(string accountId, CancellationToken ct)
    {
        await identity.DeleteAsync(accountId, ct);
        foreach (var erasure in erasures)
            await erasure.EraseAsync(accountId, ct);
    }
}
