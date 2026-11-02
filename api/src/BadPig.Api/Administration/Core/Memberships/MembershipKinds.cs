namespace BadPig.Administration.Core;

/// <summary>Kinds of membership (stored values: never change them).</summary>
public static class MembershipKinds
{
    /// <summary>The person is licensed by the club.</summary>
    public const string Internal = "internal";

    /// <summary>The person is only a member of the club, licensed in another club.</summary>
    public const string External = "external";

    public static bool IsValid(string? kind) => kind is Internal or External;
}
