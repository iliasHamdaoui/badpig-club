namespace BadPig.Interclubs.Core;

/// <summary>One of the dates of a <see cref="DateProposal"/>, with the answers of the adherents.</summary>
public sealed class ProposedDate
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public DateOnly PlayedOn { get; set; }
    public TimeOnly? StartTime { get; set; }
    public List<DateAnswer> Answers { get; set; } = [];
}
