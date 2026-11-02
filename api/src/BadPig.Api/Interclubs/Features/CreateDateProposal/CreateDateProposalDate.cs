namespace BadPig.Interclubs.Features.CreateDateProposal;

/// <summary>A date offered in a proposal.</summary>
/// <param name="Date">Day (yyyy-MM-dd).</param>
/// <param name="StartTime">Start time (HH:mm); optional.</param>
public sealed record CreateDateProposalDate(DateOnly? Date, TimeOnly? StartTime);
