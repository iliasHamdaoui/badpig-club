namespace BadPig.Interclubs.Features.GetDateProposal;

/// <summary>The answer of one person to a date.</summary>
/// <param name="AccountId">Account identifier of the person.</param>
/// <param name="Name">Name of the person; "Compte supprimé" if the account no longer exists.</param>
/// <param name="Answer">yes, maybe or no.</param>
public sealed record GetDateProposalAnswer(string AccountId, string Name, string Answer);
