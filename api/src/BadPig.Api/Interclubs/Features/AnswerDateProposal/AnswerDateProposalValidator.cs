using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.AnswerDateProposal;

internal sealed class AnswerDateProposalValidator : AbstractValidator<AnswerDateProposalRequest>
{
    public AnswerDateProposalValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Answers).Must(answers => answers is { Count: > 0 } && answers.All(IsValidEntry))
            .WithMessage("Indiquez pour chaque date : disponible (yes), peut-être (maybe) ou indisponible (no).");
    }

    private static bool IsValidEntry(AnswerDateProposalEntry entry) =>
        Guid.TryParse(entry.DateId, out _) && entry.Answer is not null && DateAnswers.All.Contains(entry.Answer);
}
