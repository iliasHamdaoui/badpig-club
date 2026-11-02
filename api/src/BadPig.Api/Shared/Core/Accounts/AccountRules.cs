
using System.Diagnostics.CodeAnalysis;

namespace BadPig.Shared.Core;

/// <summary>Rules about who may act on which account (checked in the API: the site's guards are only a display convenience).</summary>
public static class AccountRules
{
    /// <summary>True if the account belongs to the club group.</summary>
    public static bool IsInGroup(this AccountInfo account, ClubGroup group) => account.Groups.Contains(Groups.NameOf(group));

    /// <summary>Club groups of the account (the technical groups are ignored).</summary>
    public static string[] ClubGroups(AccountInfo account) => account.Groups.Where(Groups.IsClubGroup).ToArray();

    /// <summary>True if the account exists and belongs to at least one club group.</summary>
    public static bool IsClubMember([NotNullWhen(true)] AccountInfo? account) => account is not null && ClubGroups(account).Length > 0;

    public static bool IsOwnAccount(AccountInfo account, string? actorId) => account.Id == actorId;

    public static bool IsBoardOrAdmin(AccountInfo account) => account.IsInGroup(ClubGroup.Board) || account.IsInGroup(ClubGroup.Admin);

    /// <summary>
    /// True once the person has signed in at least once. Before that they are only invited: nothing says yet that the
    /// address the invitation went to is really theirs, so the club sends them nothing but that invitation.
    /// </summary>
    public static bool HasCompletedSignup(AccountInfo account) => account.LastLogin is not null;

    /// <summary>
    /// A person can manage a club account if it is not their own; a Members account can be managed by the board,
    /// Board and Admin accounts only by an Admin.
    /// </summary>
    public static bool CanManage(AccountInfo target, string? actorId, bool actorIsAdmin)
    {
        if (!IsClubMember(target) || IsOwnAccount(target, actorId))
            return false;

        return IsBoardOrAdmin(target) ? actorIsAdmin : true;
    }
}
