namespace BadPig.Interclubs.Features.CreateDateProposal;

/// <summary>Creation of a proposal of dates.</summary>
/// <param name="TeamId">Identifier of the team.</param>
/// <param name="Body">The opponent, the venue and the dates.</param>
public sealed record CreateDateProposalRequest(string TeamId, CreateDateProposalBody Body);
