using BadPig.Shared.Core;

namespace BadPig.Administration.Core;

/// <summary>Who may change the composition of the board (shared by AddToBoard, RemoveFromBoard and ListUsers).</summary>
public static class BoardRules
{
    /// <summary>
    /// What the signed-in person can do on the Board group of the target account: add, remove or nothing (null).
    /// Never on oneself; Admin accounts can only be changed by an Admin; there is always at least one board member;
    /// board access is only given to an active person who has already signed in (signup completed).
    /// </summary>
    public static string? AllowedAction(AccountInfo target, string? actorId, bool actorIsAdmin, int boardMemberCount)
    {
        if (!AccountRules.IsClubMember(target) || AccountRules.IsOwnAccount(target, actorId) || IsAdminOutOfReach(target, actorIsAdmin))
            return null;

        if (target.IsInGroup(ClubGroup.Board))
            return IsLastBoardMember(boardMemberCount) ? null : BoardActions.Remove;

        return HasCompletedSignup(target) ? BoardActions.Add : null;
    }

    private static bool IsAdminOutOfReach(AccountInfo target, bool actorIsAdmin) => target.IsInGroup(ClubGroup.Admin) && !actorIsAdmin;

    private static bool IsLastBoardMember(int boardMemberCount) => boardMemberCount <= 1;

    private static bool HasCompletedSignup(AccountInfo target) => target.IsActive && target.LastLogin is not null;
}
