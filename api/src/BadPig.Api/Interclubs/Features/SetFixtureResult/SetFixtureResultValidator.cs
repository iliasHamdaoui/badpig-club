using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.SetFixtureResult;

internal sealed class SetFixtureResultValidator : AbstractValidator<SetFixtureResultRequest>
{
    public SetFixtureResultValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Matches).Must(HaveEverySlotOnce)
            .WithMessage("Indiquez les sets des 7 matchs (SD, MX1, MX2, DD, DH, SH1, SH2), une seule fois chacun.");
        RuleFor(x => x.Body.Matches).Must(matches => matches is null || matches.All(IsValidResult))
            .WithMessage("Un match se joue en 2 ou 3 sets (15 points avec 2 points d'écart ; à partir de 14-14, 2 points d'écart ou 21 points) et s'arrête dès qu'un camp a gagné 2 sets. En cas de forfait ou d'abandon, indiquez le camp concerné.");
    }

    private static bool HaveEverySlotOnce(List<SetFixtureResultMatch>? matches) =>
        matches is not null && matches.Count == MatchSlots.All.Length
        && MatchSlots.All.All(slot => matches.Count(m => m.Slot == slot) == 1);

    private static bool IsValidResult(SetFixtureResultMatch match)
    {
        var sets = match.Sets ?? [];
        return sets.All(s => s.Club is not null && s.Opponent is not null)
            && MatchRules.IsValidEntry(match.Ending ?? MatchEndings.Played, match.Withdrawn, sets.Select(s => new SetScore(s.Club!.Value, s.Opponent!.Value)).ToList());
    }
}
