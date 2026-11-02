namespace BadPig.Interclubs.Features.AnswerDateProposal;

/// <summary>The answer to one date.</summary>
/// <param name="DateId">Identifier of the date of the proposal.</param>
/// <param name="Answer">yes (available), maybe or no.</param>
public sealed record AnswerDateProposalEntry(string? DateId, string? Answer);
