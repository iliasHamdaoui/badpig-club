using System.Security.Claims;

namespace BadPig.Shared.Core;

/// <summary>Reading of the signed-in person from the validated token (used by <c>HttpUserContext</c>; elsewhere use <see cref="IUserContext"/>) (a person is identified by "sub", never by e-mail).</summary>
internal static class ClaimsPrincipalExtensions
{
    /// <summary>account identifier of the person ("sub" claim).</summary>
    public static string? AccountId(this ClaimsPrincipal user) => user.FindFirstValue("sub");

    public static string DisplayName(this ClaimsPrincipal user) =>
        user.FindFirstValue("name") ?? user.FindFirstValue("preferred_username") ?? "?";
}
