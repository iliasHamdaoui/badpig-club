namespace BadPig.Interclubs.Features.ListDateProposals;

/// <summary>Values of the "status" filter of the proposals.</summary>
internal static class ProposalStatuses
{
    public const string Open = "open";
    public const string Chosen = "chosen";

    public static readonly string[] All = [Open, Chosen];
}
