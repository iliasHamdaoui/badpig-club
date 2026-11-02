namespace BadPig.Shared.Core;

/// <summary>Names of the authorization policies (registered by the host, used by the endpoints of every module).</summary>
public static class Policies
{
    /// <summary>Any club group: Members, Board or Admin.</summary>
    public const string Member = "Member";

    /// <summary>Board or Admin.</summary>
    public const string Board = "Board";

    public const string Admin = "Admin";
}
