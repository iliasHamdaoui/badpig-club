namespace BadPig.Interclubs.Features.ChooseProposedDate;

/// <summary>Choice of the date of the fixture.</summary>
/// <param name="Id">Identifier of the proposal.</param>
/// <param name="Body">The chosen date.</param>
public sealed record ChooseProposedDateRequest(string Id, ChooseProposedDateBody Body);
