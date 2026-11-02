namespace BadPig.Interclubs.Core;

/// <summary>The seven matches of a fixture, in the usual order: women's singles, two mixed doubles, women's doubles, men's doubles, two men's singles.</summary>
public static class MatchSlots
{
    public const string WomenSingles = "SD";
    public const string Mixed1 = "MX1";
    public const string Mixed2 = "MX2";
    public const string WomenDoubles = "DD";
    public const string MenDoubles = "DH";
    public const string MenSingles1 = "SH1";
    public const string MenSingles2 = "SH2";

    public static readonly string[] All = [WomenSingles, Mixed1, Mixed2, WomenDoubles, MenDoubles, MenSingles1, MenSingles2];

    /// <summary>1 player for a singles match, 2 for a doubles or mixed one.</summary>
    public static int PlayersIn(string slot) => slot is WomenSingles or MenSingles1 or MenSingles2 ? 1 : 2;
}
