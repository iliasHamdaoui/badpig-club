using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.SetMatchResult;

internal sealed class SetMatchResultValidator : AbstractValidator<SetMatchResultRequest>
{
    public SetMatchResultValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Slot).Must(slot => slot is not null && MatchSlots.All.Contains(slot))
            .WithMessage($"Match inconnu : {string.Join(", ", MatchSlots.All)}.");
        RuleFor(x => x.Body).Must(IsValidResult)
            .WithMessage("Un match se joue en 2 ou 3 sets (15 points avec 2 points d'écart ; à partir de 14-14, 2 points d'écart ou 21 points) et s'arrête dès qu'un camp a gagné 2 sets. En cas de forfait ou d'abandon, indiquez le camp concerné.");
    }

    private static bool IsValidResult(SetMatchResultBody body)
    {
        var sets = body.Sets ?? [];
        return sets.All(s => s.Club is not null && s.Opponent is not null)
            && MatchRules.IsValidEntry(body.Ending ?? MatchEndings.Played, body.Withdrawn, sets.Select(s => new SetScore(s.Club!.Value, s.Opponent!.Value)).ToList());
    }
}
