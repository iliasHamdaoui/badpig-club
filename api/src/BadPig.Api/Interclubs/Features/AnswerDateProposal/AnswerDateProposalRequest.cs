namespace BadPig.Interclubs.Features.AnswerDateProposal;

/// <summary>Answers of the signed-in person.</summary>
/// <param name="Id">Identifier of the proposal.</param>
/// <param name="Body">The answers.</param>
public sealed record AnswerDateProposalRequest(string Id, AnswerDateProposalBody Body);
