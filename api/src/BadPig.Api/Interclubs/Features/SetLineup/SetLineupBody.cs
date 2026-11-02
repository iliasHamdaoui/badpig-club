namespace BadPig.Interclubs.Features.SetLineup;

/// <summary>JSON body of the lineup.</summary>
/// <param name="Slots">The matches that have players; the other ones are emptied.</param>
public sealed record SetLineupBody(List<SetLineupSlot>? Slots);
