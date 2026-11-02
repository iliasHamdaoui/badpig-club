namespace BadPig.Shared.Core;

/// <summary>Names of the club groups in the identity provider (they appear in the "groups" claim of the token).</summary>
public static class Groups
{
    public const string Members = "Membres";
    public const string Board = "Bureau";
    public const string Admin = "Admin";

    public static bool IsClubGroup(string name) => name is Members or Board or Admin;

    public static string NameOf(ClubGroup group) => group switch
    {
        ClubGroup.Members => Members,
        ClubGroup.Board => Board,
        ClubGroup.Admin => Admin,
        _ => throw new ArgumentOutOfRangeException(nameof(group)),
    };
}
