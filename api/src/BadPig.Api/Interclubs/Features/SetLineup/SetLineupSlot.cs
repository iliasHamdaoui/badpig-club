namespace BadPig.Interclubs.Features.SetLineup;

/// <summary>The players of one match.</summary>
/// <param name="Slot">SD, MX1, MX2, DD, DH, SH1 or SH2.</param>
/// <param name="PlayerIds">Account identifiers of the players: one for SD, SH1, SH2; two (different) for MX1, MX2, DD, DH.</param>
public sealed record SetLineupSlot(string? Slot, List<string>? PlayerIds);
