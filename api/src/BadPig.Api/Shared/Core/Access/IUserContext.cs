namespace BadPig.Shared.Core;

/// <summary>
/// The signed-in person of the current request, as read from the validated token. Code outside the HTTP layer
/// (use cases, services) depends on this interface instead of reading <c>ClaimsPrincipal</c> directly.
/// A person is identified by their account identifier ("sub"), never by e-mail.
/// </summary>
public interface IUserContext
{
    /// <summary>account identifier of the person; null when nobody is signed in (e.g. a background task).</summary>
    string? AccountId { get; }

    string DisplayName { get; }

    /// <summary>True if the signed-in person belongs to the club group (from the token).</summary>
    bool IsInGroup(ClubGroup group);
}
