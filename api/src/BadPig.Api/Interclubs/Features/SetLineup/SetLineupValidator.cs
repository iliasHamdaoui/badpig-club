using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.SetLineup;

internal sealed class SetLineupValidator : AbstractValidator<SetLineupRequest>
{
    public SetLineupValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Slots).Must(slots => slots is not null && slots.All(IsValidSlot) && slots.Select(s => s.Slot).Distinct().Count() == slots.Count)
            .WithMessage("Composition invalide : chaque match (SD, MX1, MX2, DD, DH, SH1, SH2) une seule fois, avec un joueur pour un simple et deux joueurs différents pour un double.");
    }

    private static bool IsValidSlot(SetLineupSlot slot) =>
        slot.Slot is not null && MatchSlots.All.Contains(slot.Slot)
        && slot.PlayerIds is not null && slot.PlayerIds.Count == MatchSlots.PlayersIn(slot.Slot)
        && slot.PlayerIds.All(id => Guid.TryParse(id, out _))
        && slot.PlayerIds.Select(id => Guid.Parse(id)).Distinct().Count() == slot.PlayerIds.Count;
}
