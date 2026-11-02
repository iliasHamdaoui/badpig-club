namespace BadPig.Interclubs.Features.GetDateProposal;

/// <summary>A proposal of dates.</summary>
/// <param name="Id">Identifier of the proposal.</param>
/// <param name="TeamId">Identifier of the team.</param>
/// <param name="TeamName">Name of the team, e.g. "D4".</param>
/// <param name="Opponent">The opposing club.</param>
/// <param name="Venue">home (at the club) or away.</param>
/// <param name="Place">Gym or address; null when not given.</param>
/// <param name="IsOpen">True while the captain has not chosen a date.</param>
/// <param name="FixtureId">Identifier of the fixture created from the chosen date; null while the proposal is open.</param>
/// <param name="CanAnswer">True when the signed-in person is a player of the team and the proposal is still open.</param>
/// <param name="CanManage">True when the signed-in person can choose a date or delete the proposal (captain of the team, or board).</param>
/// <param name="Dates">The dates offered, from the earliest.</param>
public sealed record GetDateProposalResponse(string Id, string TeamId, string TeamName, string Opponent, string Venue, string? Place, bool IsOpen, string? FixtureId, bool CanAnswer, bool CanManage, IReadOnlyList<GetDateProposalDate> Dates);
