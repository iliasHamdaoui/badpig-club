using BadPig.Administration.Core;
using BadPig.Shared.Core;

namespace BadPig.Interclubs.Core;

/// <summary>
/// Who can do what in the interclubs: the board and the members whose membership is up to date can look (same rule as the members area);
/// the captain of a team, or the board, manages its fixtures.
/// </summary>
public sealed class InterclubAccess(IUserContext currentUser, IMembershipReader memberships)
{
    public async Task<bool> CanViewAsync(CancellationToken ct)
    {
        if (IsOnTheBoard)
            return true;

        if (IsNotAMember)
            return false;

        return (await memberships.GetAsync(currentUser.AccountId!, ct)).State != MembershipStates.ToValidate;
    }

    /// <summary>The board, or the captain of this team, can change its fixtures.</summary>
    public bool CanManageFixtures(Team team) => IsOnTheBoard || IsCaptainOf(team);

    public bool IsCaptainOf(Team team) =>
        currentUser.AccountId is not null && team.CaptainId?.ToString() == currentUser.AccountId;

    private bool IsOnTheBoard => currentUser.IsInGroup(ClubGroup.Board) || currentUser.IsInGroup(ClubGroup.Admin);

    private bool IsNotAMember => currentUser.AccountId is null || !currentUser.IsInGroup(ClubGroup.Members);
}
