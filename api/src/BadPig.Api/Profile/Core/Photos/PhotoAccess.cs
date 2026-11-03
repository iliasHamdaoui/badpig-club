using BadPig.Administration.Core;
using BadPig.Shared.Core;

namespace BadPig.Profile.Core;

/// <summary>Who may look at the pictures of the members: the board, and the members whose membership is up to date (same rule as the members area).</summary>
public sealed class PhotoAccess(IUserContext currentUser, IMembershipReader memberships)
{
    public async Task<bool> CanViewAsync(CancellationToken ct)
    {
        if (IsOnTheBoard)
            return true;

        if (IsNotAMember)
            return false;

        return (await memberships.GetAsync(currentUser.AccountId!, ct)).State != MembershipStates.ToValidate;
    }

    private bool IsOnTheBoard => currentUser.IsInGroup(ClubGroup.Board) || currentUser.IsInGroup(ClubGroup.Admin);

    private bool IsNotAMember => currentUser.AccountId is null || !currentUser.IsInGroup(ClubGroup.Members);
}
