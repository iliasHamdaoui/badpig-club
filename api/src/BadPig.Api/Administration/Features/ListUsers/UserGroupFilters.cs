using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListUsers;

/// <summary>
/// The "group" filter of the member list. Everybody is in Members, so filtering on it would show the whole club:
/// <c>members</c> therefore means the plain members, those who are neither on the board nor administrators.
/// </summary>
internal static class UserGroupFilters
{
    public const string Members = "members";
    public const string Board = "board";
    public const string Admin = "admin";

    public static readonly string[] All = [Members, Board, Admin];

    public static bool IsKnown(string? filter) => string.IsNullOrWhiteSpace(filter) || All.Contains(filter.ToLowerInvariant());

    public static bool Matches(AccountInfo account, string? filter) => filter?.ToLowerInvariant() switch
    {
        Members => !AccountRules.IsBoardOrAdmin(account),
        Board => account.IsInGroup(ClubGroup.Board),
        Admin => account.IsInGroup(ClubGroup.Admin),
        _ => true,
    };
}
