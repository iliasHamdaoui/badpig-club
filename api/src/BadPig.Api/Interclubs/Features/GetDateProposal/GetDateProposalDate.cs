namespace BadPig.Interclubs.Features.GetDateProposal;

/// <summary>A date of a proposal.</summary>
/// <param name="Id">Identifier of the date.</param>
/// <param name="Date">Day (yyyy-MM-dd).</param>
/// <param name="StartTime">Start time (HH:mm:ss); null when not known.</param>
/// <param name="Yes">Number of people available.</param>
/// <param name="Maybe">Number of people who maybe are.</param>
/// <param name="No">Number of people not available.</param>
/// <param name="MyAnswer">Answer of the signed-in person: yes, maybe or no; null when they have not answered.</param>
/// <param name="Answers">Who answered what; empty unless the signed-in person can manage the proposal.</param>
public sealed record GetDateProposalDate(string Id, DateOnly Date, TimeOnly? StartTime, int Yes, int Maybe, int No, string? MyAnswer, IReadOnlyList<GetDateProposalAnswer> Answers);
