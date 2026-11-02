namespace BadPig.Administration.Core;

public static class MembershipStates
{
    /// <summary>Membership of the current season is validated.</summary>
    public const string Valid = "valid";

    /// <summary>September grace period: the membership of the previous season is still accepted.</summary>
    public const string Grace = "grace";

    public const string ToValidate = "to_validate";
}
