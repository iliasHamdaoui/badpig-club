using BadPig.Administration.Core;

namespace BadPig.Administration.Features.ListUsers;

/// <summary>One row of the administration user list, with what the signed-in person can do on this account.</summary>
/// <param name="Id">Account identifier (use it in the routes of the other administration endpoints).</param>
/// <param name="Username">Sign-in name (the e-mail address for invited people).</param>
/// <param name="Name">Full name.</param>
/// <param name="Email">E-mail address.</param>
/// <param name="IsActive">False when the account is disabled (it cannot sign in).</param>
/// <param name="Groups">Club groups of the person: <c>members</c>, <c>board</c>, <c>admin</c>.</param>
/// <param name="CanManage">True when the signed-in person can disable, enable or delete this account.</param>
/// <param name="Status">Account status: <c>active</c>, <c>disabled</c>, or <c>pending</c> (has never signed in: the person has not chosen a password yet).</param>
/// <param name="InvitedByName">Name of the person who sent the invitation, when the account was invited.</param>
/// <param name="InvitedAt">Date of the invitation, when the account was invited.</param>
/// <param name="BoardAction">What the signed-in person can do about the board for this account: <c>add</c>, <c>remove</c>, or null when nothing.</param>
/// <param name="Membership">Membership state: <c>valid</c> (current season validated), <c>grace</c> (September: previous season still accepted) or <c>to_validate</c>.</param>
/// <param name="MembershipSeason">Name of the latest validated season (for example 2026-2027), or null.</param>
/// <param name="MembershipKind">Kind of membership: <c>internal</c> (licensed by the club) or <c>external</c> (licensed in another club). Latest validated membership; without one, the kind chosen at the invitation (<c>internal</c> by default).</param>
/// <param name="MembershipAmount">Fee recorded with the latest validated membership, in euros, or null.</param>
/// <param name="MembershipValidatedAt">Date the latest membership was validated, or null.</param>
/// <param name="MembershipValidatedByName">Name of the board member who validated it, or null.</param>
/// <param name="CanValidateMembership">True when the signed-in person can validate or cancel this membership (never for oneself).</param>
/// <param name="Licence">Federation licence number (FFBaD) on record, or null.</param>
/// <param name="TitleId">Identifier of the board title held by the person, or null.</param>
/// <param name="TitleLabel">Name of the board title held by the person (for example Président), or null.</param>
/// <param name="AutomaticDeletionAt">Date the disabled account will be deleted automatically, or null (active, board and admin accounts).</param>
public sealed record ListUsersItem(
    string Id,
    string Username,
    string Name,
    string Email,
    bool IsActive,
    string[] Groups,
    bool CanManage,
    string Status,
    string? InvitedByName,
    DateTimeOffset? InvitedAt,
    string? BoardAction,
    string Membership,
    string? MembershipSeason,
    string MembershipKind,
    decimal? MembershipAmount,
    DateTimeOffset? MembershipValidatedAt,
    string? MembershipValidatedByName,
    bool CanValidateMembership,
    string? Licence,
    string? TitleId,
    string? TitleLabel,
    DateTimeOffset? AutomaticDeletionAt);
