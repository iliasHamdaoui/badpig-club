namespace BadPig.Interclubs.Features.AnswerDateProposal;

/// <summary>JSON body of the answers to a proposal.</summary>
/// <param name="Answers">One entry per date answered (at least one).</param>
public sealed record AnswerDateProposalBody(List<AnswerDateProposalEntry>? Answers);
