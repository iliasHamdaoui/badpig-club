namespace BadPig.Interclubs.Features.CreateDateProposal;

/// <summary>JSON body of the creation of a proposal of dates.</summary>
/// <param name="Opponent">The opposing club (1 to 100 characters).</param>
/// <param name="Venue">home (at the club) or away.</param>
/// <param name="Place">Gym or address (200 characters at most); optional.</param>
/// <param name="Dates">The dates offered: two at least, ten at most, all different.</param>
public sealed record CreateDateProposalBody(string? Opponent, string? Venue, string? Place, List<CreateDateProposalDate>? Dates);
