using BadPig.Shared.Core;
using BadPig.Shared.Identity;

namespace BadPig.Administration.Core;

/// <summary>Loads the target account of a management action and checks that the signed-in person may act on it.</summary>
public sealed class AccountAccess(IIdentityProvider identity, IUserContext currentUser)
{
    public async Task<Result<AccountInfo>> ResolveManageableAsync(string id, CancellationToken ct)
    {
        var target = await identity.FindAsync(id, ct);
        if (!AccountRules.IsClubMember(target))
            return Error.NotFound();

        if (!AccountRules.CanManage(target, currentUser.AccountId, currentUser.IsInGroup(ClubGroup.Admin)))
            return Error.Forbidden("Vous ne pouvez pas agir sur ce compte.");

        return target;
    }
}
