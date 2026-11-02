namespace BadPig.Profile.Features.GetMyProfile;

/// <summary>Profile of the signed-in person.</summary>
/// <param name="Id">Account identifier.</param>
/// <param name="Username">Sign-in name.</param>
/// <param name="Name">Full name.</param>
/// <param name="Email">E-mail address.</param>
/// <param name="Groups">Club groups of the person: <c>members</c>, <c>board</c>, <c>admin</c>.</param>
/// <param name="Membership">Membership state: <c>valid</c>, <c>grace</c> or <c>to_validate</c>.</param>
/// <param name="MembershipUpToDate">True when the members area is accessible (always true for board and admins).</param>
/// <param name="Title">Board title held by the person (for example Président), or null.</param>
/// <param name="Licence">Federation licence number (FFBaD) on record, or null.</param>
/// <param name="MemberSince">Day the person joined the club (their first validated membership), or null.</param>
public sealed record GetMyProfileResponse(
    string Id,
    string Username,
    string Name,
    string Email,
    string[] Groups,
    string Membership,
    bool MembershipUpToDate,
    string? Title,
    string? Licence,
    DateTimeOffset? MemberSince);
