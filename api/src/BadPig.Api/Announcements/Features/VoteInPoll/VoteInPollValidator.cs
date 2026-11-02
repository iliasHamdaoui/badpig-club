using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.VoteInPoll;

internal sealed class VoteInPollValidator : AbstractValidator<VoteInPollRequest>
{
    public VoteInPollValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.OptionIds).NotNull().WithMessage("Choisissez au moins une réponse.")
            .Must(ids => ids is null || ids.Length > 0).WithMessage("Choisissez au moins une réponse.")
            .Must(ids => ids is null || ids.All(id => Guid.TryParse(id, out _))).WithMessage("Choix inconnu.")
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Length).WithMessage("Un choix ne peut être coché qu'une fois.");
    }
}
