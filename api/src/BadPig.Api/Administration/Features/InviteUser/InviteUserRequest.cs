namespace BadPig.Administration.Features.InviteUser;

/// <summary>Body of the invitation.</summary>
/// <param name="Name">Full name of the person (1 to 200 characters).</param>
/// <param name="Email">E-mail address (it becomes the username).</param>
/// <param name="Group">"members" (default), "board" or "admin" (only an Admin can invite an administrator).</param>
/// <param name="MembershipKind">Kind of membership: <c>internal</c> (licensed by the club, default) or <c>external</c> (licensed in another club). It decides the fee when the board validates the membership.</param>
public sealed record InviteUserRequest(
    string? Name,
    string? Email,
    string? Group = null,
    string? MembershipKind = null);
