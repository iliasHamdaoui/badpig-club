namespace BadPig.Interclubs.Features.ChooseProposedDate;

/// <summary>JSON body of the choice of a date.</summary>
/// <param name="DateId">Identifier of the chosen date of the proposal.</param>
public sealed record ChooseProposedDateBody(string? DateId);
